using System;
using Framework.Core.Animations;
using Framework.Core.Messaging;
using Framework.Core.Services;
using Framework.Physics;
using Framework.Physics.RigidBodies;
using Framework.Physics.RigidBodies.Shapes;
using Game.Entities;
using Game.Entities.Outworld.Characters;
using Game.Entities.Outworld.World;
using Game.Entities.Outworld.World.SpatialSensor;
using Game.Entities.System;
using Game.Entities.System.ComponentModel;
using Game.Entities.System.EntityModel;
using Game.Network.Clients;
using Game.Network.Common;
using Game.World.Terrains.Helpers;
using Game.World.Terrains.Parts.Areas.Helpers;
using Game.World.Terrains.Parts.Tiles;
using Microsoft.Xna.Framework;
using Outworld.Scenes.InGame;
using Outworld.Settings.Global;

namespace Outworld.Players
{
	public class PlayerComponent : IComponent, IModelUpdateable
	{
		public string Id { get; set; }
		public IEntity Owner { get; set; }

		// Custom properties
		private AnimationHandler<AnimationType> animationHandler;
		public bool IsDead { get; private set; }
		public bool IsCrouching;
		public float CameraOffsetY;

		// Components
		private SpatialComponent spatial;
		private SpatialSensorComponent spatialSensor;
		private HealthComponent healthComponent;
		private PlayerInputComponent inputComponent;
		private IMessageHandler messageHandler;

		private IGameClient gameClient;
		private IPhysicsHandler physicsHandler;
		private TerrainContextCollisionHelper terrainContextCollisionHelper;
		private GlobalSettings globalSettings;
		private bool isDying;
		private EntityEventType entityEvent;
		private EntityEventType previousEntityEvent;

		private byte collisionHandlerCounter;

		public PlayerComponent(IGameClient gameClient, IPhysicsHandler physicsHandler, TerrainContextCollisionHelper terrainContextCollisionHelper)
		{
			this.gameClient = gameClient;
			this.physicsHandler = physicsHandler;
			this.terrainContextCollisionHelper = terrainContextCollisionHelper;

			globalSettings = ServiceLocator.Get<GlobalSettings>();
			messageHandler = ServiceLocator.Get<IMessageHandler>();

			CameraOffsetY = globalSettings.Player.CameraOffsetY;
			float cameraCrouchingOffsetY = globalSettings.Player.CameraCrouchingOffsetY;
			int crouchDuration = globalSettings.Player.AnimationDuration.Crouch;
			int standDuration = globalSettings.Player.AnimationDuration.Stand;
			int deathDuration = globalSettings.Player.AnimationDuration.Death;

			// Initialize the animations
			animationHandler = new AnimationHandler<AnimationType>();
			animationHandler.Animations.Add(AnimationType.Crouch, new Animation(CameraOffsetY, cameraCrouchingOffsetY, crouchDuration));
			animationHandler.Animations.Add(AnimationType.Stand, new Animation(cameraCrouchingOffsetY, CameraOffsetY, standDuration));
			animationHandler.Animations.Add(AnimationType.DeathCameraRoll, new Animation(0, 90, deathDuration));
			animationHandler.Animations.Add(AnimationType.DeathCameraTilt, new Animation(0, 0, deathDuration));
			animationHandler.Animations.Add(AnimationType.DeathCameraOffsetY, new Animation(CameraOffsetY, cameraCrouchingOffsetY, deathDuration));

			collisionHandlerCounter = 0;
		}

		public void Initialize()
		{
			spatial = Owner.Components.Get<SpatialComponent>();
			spatialSensor = Owner.Components.Get<SpatialSensorComponent>();
			healthComponent = Owner.Components.Get<HealthComponent>();
			inputComponent = Owner.Components.Get<PlayerInputComponent>();
		}

		public void Update(GameTime gameTime)
		{
			AreaHelper.FindAreaLocation(spatial.Position, ref spatial.Area);

			if (isDying)
			{
				HandleDying();
			}
			else
			{
				UpdateMovement();

				// Crouching
				if (animationHandler.Animations[AnimationType.Crouch].IsRunning)
				{
					CameraOffsetY = animationHandler.Animations[AnimationType.Crouch].CurrentValue;
				}
				// Standing up
				else if (animationHandler.Animations[AnimationType.Stand].IsRunning)
				{
					CameraOffsetY = animationHandler.Animations[AnimationType.Stand].CurrentValue;
				}

				// Check impacts
				if (spatialSensor.State[SpatialSensorStateType.Impact].IsActive)
				{
					float impactDepth = (1.0f + spatialSensor.State[SpatialSensorStateType.Impact].Value.Y);
					float damage = (float)Math.Pow(impactDepth, 8);
					healthComponent.Subtract(Math.Abs(damage * 2f));

					if (healthComponent.Health == 0)
					{
						Kill();
					}
				}
			}

			if (collisionHandlerCounter++ > 2)
			{
				HandleCollisions();
				collisionHandlerCounter = 0;
			}

			animationHandler.Update();
		}

		private void HandleDying()
		{
			if (animationHandler.Animations[AnimationType.DeathCameraRoll].IsRunning)
			{
				spatial.Angle.Z = animationHandler.Animations[AnimationType.DeathCameraRoll].CurrentValue;
			}

			if (animationHandler.Animations[AnimationType.DeathCameraTilt].IsRunning)
			{
				spatial.Angle.Y = animationHandler.Animations[AnimationType.DeathCameraTilt].CurrentValue;
			}

			if (animationHandler.Animations[AnimationType.DeathCameraOffsetY].IsRunning)
			{
				CameraOffsetY = animationHandler.Animations[AnimationType.DeathCameraOffsetY].CurrentValue;
			}

			isDying = animationHandler.HasRunningAnimations;
		}

		private void UpdateMovement()
		{
			// Running
			if (spatialSensor.State[SpatialSensorStateType.HorizontalMovement].IsActive)
			{
				// Calculate the new direction enum value
				int newDirection = (int)EntityEventType.RunDirection1 + (inputComponent.MovementDirection - 1);
				entityEvent = (EntityEventType)newDirection;
			}
			// Idle
			else
			{
				entityEvent = EntityEventType.Idle;
			}

			if (entityEvent != previousEntityEvent)
			{
				messageHandler.AddMessage(MessageHandlerType.ServerEntityEvents, GetPlayerMessage(entityEvent));
			}

			previousEntityEvent = entityEvent;
		}

		public void Kill()
		{
			inputComponent.IsEnabled = false;

			// Stop the player from moving on the x and z axes when dead
			spatial.Velocity = new Vector3(0, spatial.Velocity.Y, 0);

			IsDead = true;
			isDying = true;

			animationHandler.Animations[AnimationType.DeathCameraTilt].From = spatial.Angle.Y;
			animationHandler.Animations[AnimationType.DeathCameraRoll].Start();
			animationHandler.Animations[AnimationType.DeathCameraTilt].Start();
			animationHandler.Animations[AnimationType.DeathCameraOffsetY].Start();

			messageHandler.AddMessage(MessageHandlerType.ServerEntityEvents, GetPlayerMessage(EntityEventType.Dead));
		}

		public void ToggleStandCrouch()
		{
			// If there are no animations running
			if (!animationHandler.HasRunningAnimations)
			{
				if (!IsCrouching)
				{
					animationHandler.Animations[AnimationType.Crouch].Start();
					IsCrouching = true;
				}
				else
				{
					animationHandler.Animations[AnimationType.Stand].Start();
					IsCrouching = false;
				}
			}
		}

		private void HandleCollisions()
		{
			var bounds = spatial.GetBoundingBox(globalSettings.Player.Spatial.CollisionDetectionBounds, false);

			// Gets all tiles that the player intersects at the target position
			var intersectingTiles = terrainContextCollisionHelper.GetIntersectingTiles(bounds);

			if (intersectingTiles.Count > 0)
			{
				physicsHandler.RigidBodies.RemoveGroup("Terrain");

				for (int i = 0; i < intersectingTiles.Count; i++)
				{
					physicsHandler.RigidBodies.Add(new RigidBody(intersectingTiles[i].Id, "Terrain")
					{
						IsStatic = true,
						Shape = new BoxShape(Tile.Size.X, Tile.Size.Y, Tile.Size.Z),
						Position = new Vector3(intersectingTiles[i].BoundingBox.Min.X + Tile.HalfSize.X,
											   intersectingTiles[i].BoundingBox.Min.Y + Tile.HalfSize.Y,
											   intersectingTiles[i].BoundingBox.Min.Z + Tile.HalfSize.Z)
					});
				}
			}
		}

		private PlayerMessage GetPlayerMessage(EntityEventType type)
		{
			var message = new PlayerMessage();
			message.EntityEvent = new EntityEvent();
			message.EntityEvent.TimeStamp = gameClient.TimeStamp;
			message.EntityEvent.Type = type;

			return message;
		}
	}
}