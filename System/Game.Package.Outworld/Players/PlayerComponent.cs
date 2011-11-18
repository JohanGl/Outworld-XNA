using System;
using Framework.Core.Animations;
using Framework.Core.Services;
using Framework.Physics;
using Framework.Physics.RigidBodies;
using Framework.Physics.RigidBodies.Shapes;
using Game.Entities.Outworld.Characters;
using Game.Entities.Outworld.World;
using Game.Entities.Outworld.World.SpatialSensor;
using Game.Entities.System;
using Game.Entities.System.ComponentModel;
using Game.Entities.System.EntityModel;
using Game.World.Terrains.Helpers;
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
		public AnimationHandler<AnimationType> AnimationHandler { get; set; }
		public bool IsDead { get; private set; }
		public bool IsCrouching;
		public float CameraOffsetY;

		// Components
		private SpatialComponent spatial;
		private SpatialSensorComponent spatialSensor;
		private HealthComponent health;
		private PlayerInputComponent input;

		private IPhysicsHandler physicsHandler;
		private TerrainContextCollisionHelper terrainContextCollisionHelper;
		private GlobalSettings globalSettings;
		private bool isDying;

		private byte collisionHandlerCounter;

		public PlayerComponent(IPhysicsHandler physicsHandler, TerrainContextCollisionHelper terrainContextCollisionHelper)
		{
			this.physicsHandler = physicsHandler;
			this.terrainContextCollisionHelper = terrainContextCollisionHelper;

			globalSettings = ServiceLocator.Get<GlobalSettings>();

			CameraOffsetY = globalSettings.Player.CameraOffsetY;
			float cameraCrouchingOffsetY = globalSettings.Player.CameraCrouchingOffsetY;
			int crouchDuration = globalSettings.Player.AnimationDuration.Crouch;
			int standDuration = globalSettings.Player.AnimationDuration.Stand;
			int deathDuration = globalSettings.Player.AnimationDuration.Death;

			// Initialize the animations
			AnimationHandler = new AnimationHandler<AnimationType>();
			AnimationHandler.Animations.Add(AnimationType.Crouch, new Animation(CameraOffsetY, cameraCrouchingOffsetY, crouchDuration));
			AnimationHandler.Animations.Add(AnimationType.Stand, new Animation(cameraCrouchingOffsetY, CameraOffsetY, standDuration));
			AnimationHandler.Animations.Add(AnimationType.DeathCameraRoll, new Animation(0, 90, deathDuration));
			AnimationHandler.Animations.Add(AnimationType.DeathCameraTilt, new Animation(0, 0, deathDuration));
			AnimationHandler.Animations.Add(AnimationType.DeathCameraOffsetY, new Animation(CameraOffsetY, cameraCrouchingOffsetY, deathDuration));

			collisionHandlerCounter = 0;
		}

		public void Initialize()
		{
			// Spatial
			spatial = Owner.Components.Get<SpatialComponent>();

			// Spatial sensor
			spatialSensor = Owner.Components.Get<SpatialSensorComponent>();

			// Health
			health = Owner.Components.Get<HealthComponent>();

			// Input
			input = Owner.Components.Get<PlayerInputComponent>();
		}

		public void Update(GameTime gameTime)
		{
			// Handle death animations
			if (isDying)
			{
				if (AnimationHandler.Animations[AnimationType.DeathCameraRoll].IsRunning)
				{
					spatial.Angle.Z = AnimationHandler.Animations[AnimationType.DeathCameraRoll].CurrentValue;
				}

				if (AnimationHandler.Animations[AnimationType.DeathCameraTilt].IsRunning)
				{
					spatial.Angle.Y = AnimationHandler.Animations[AnimationType.DeathCameraTilt].CurrentValue;
				}

				if (AnimationHandler.Animations[AnimationType.DeathCameraOffsetY].IsRunning)
				{
					CameraOffsetY = AnimationHandler.Animations[AnimationType.DeathCameraOffsetY].CurrentValue;
				}

				isDying = AnimationHandler.HasRunningAnimations;
			}

			// Crouching
			if (AnimationHandler.Animations[AnimationType.Crouch].IsRunning)
			{
				CameraOffsetY = AnimationHandler.Animations[AnimationType.Crouch].CurrentValue;
			}
			// Standing up
			else if (AnimationHandler.Animations[AnimationType.Stand].IsRunning)
			{
				CameraOffsetY = AnimationHandler.Animations[AnimationType.Stand].CurrentValue;
			}

			// Check impacts
			if (spatialSensor.State[SpatialSensorState.Impact])
			{
				float impactDepth = (1.0f + spatialSensor.ImpactDepth.Y);
				float damage = (float)Math.Pow(impactDepth, 8);
				health.Subtract(Math.Abs(damage * 2f));

				if (health.Health == 0)
				{
					Kill();
				}
			}

			if (collisionHandlerCounter++ > 2)
			{
				HandleCollisions();
				collisionHandlerCounter = 0;
			}
			
			AnimationHandler.Update();
		}

		public void Kill()
		{
			input.IsEnabled = false;
			isDying = true;

			AnimationHandler.Animations[AnimationType.DeathCameraTilt].From = spatial.Angle.Y;
			AnimationHandler.Animations[AnimationType.DeathCameraRoll].Start();
			AnimationHandler.Animations[AnimationType.DeathCameraTilt].Start();
			AnimationHandler.Animations[AnimationType.DeathCameraOffsetY].Start();
		}

		public void ToggleStandCrouch()
		{
			// If there are no animations running
			if (!AnimationHandler.HasRunningAnimations)
			{
				if (!IsCrouching)
				{
					AnimationHandler.Animations[AnimationType.Crouch].Start();
					IsCrouching = true;
				}
				else
				{
					AnimationHandler.Animations[AnimationType.Stand].Start();
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
	}
}