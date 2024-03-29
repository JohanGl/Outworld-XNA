﻿using System;
using Framework.Core.Contexts;
using Framework.Core.Services;
using Framework.Physics.RigidBodies;
using Framework.Physics.RigidBodies.Shapes;
using Game.Entities.Outworld;
using Game.Entities.Outworld.Characters;
using Game.Entities.Outworld.World;
using Game.Entities.Outworld.World.SpatialSensor;
using Game.Network.Clients;
using Game.World;
using Outworld.Players;
using Outworld.Settings.Global;

namespace Outworld.Helpers.EntityFactories
{
	/// <summary>
	/// Instantiates player entities
	/// </summary>
	public class PlayerEntityFactory
	{
		public static Entity Get(string id, GameContext context, WorldContext worldContext)
		{
			var player = new Entity();

			var globalSettings = ServiceLocator.Get<GlobalSettings>();
			var gameClient = ServiceLocator.Get<IGameClient>();

			// Spatial
			var spatial = new SpatialComponent()
			{
				Owner = player,
				Angle = globalSettings.Player.Spatial.Angle,
				Area = globalSettings.Player.Spatial.Area,
				CollisionDetectionBounds = globalSettings.Player.Spatial.CollisionDetectionBounds
			};

			// Spatial rigid body
			var rigidBodyPlayer = new RigidBody(id, "Players")
			{
				Shape = new CapsuleShape(globalSettings.Player.Spatial.Size.Y * 0.5f, Math.Max(globalSettings.Player.Spatial.Size.X, globalSettings.Player.Spatial.Size.Z)),
				RigidBodyHandler = worldContext.PhysicsHandler.RigidBodies
			};

			spatial.RigidBody = rigidBodyPlayer;
			worldContext.PhysicsHandler.RigidBodies.Add(rigidBodyPlayer);
			worldContext.PhysicsHandler.RigidBodies.CanRotate(rigidBodyPlayer, false);
			spatial.Position = globalSettings.Player.Spatial.Position;
			player.Components.Add(spatial);

			// Spatial sensor
			var spatialSensor = new SpatialSensorComponent() { Owner = player };
			player.Components.Add(spatialSensor);

			// Health
			var health = new HealthComponent() { Owner = player };
			player.Components.Add(health);

			// Input
			var playerInput = new PlayerInputComponent() { Owner = player };
			player.Components.Add(playerInput);

			// Player
			var playerComponent = new PlayerComponent(gameClient, worldContext.PhysicsHandler, worldContext.TerrainContext.TerrainContextCollisionHelper) { Owner = player };
			player.Components.Add(playerComponent);

			// Initialize all components
			spatialSensor.Initialize();
			health.Initialize(globalSettings.Player.Health, globalSettings.Player.MaxHealth);
			playerComponent.Initialize();
			playerInput.Initialize(context.Input);

			return player;
		}
	}
}