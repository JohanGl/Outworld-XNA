﻿using System;
using System.Collections.Generic;
using Framework.Core.Diagnostics.Logging;
using Framework.Core.Messaging;
using Framework.Core.Services;
using Framework.Network.Clients;
using Framework.Network.Clients.Configurations;
using Game.Entities;
using Game.Network.Clients.Settings;
using Game.Network.Common;
using Game.World;
using Microsoft.Xna.Framework;

namespace Game.Network.Clients
{
	public partial class GameClient : IGameClient
	{
		public bool IsConnected
		{
			get
			{
				return client.IsConnected;
			}
		}

		public float TimeStamp
		{
			get
			{
				return client.TimeStamp;
			}
		}

		public WorldContext World { get; set; }
		public ushort ClientId { get; set; }
		public List<ServerEntity> ServerEntities { get; set; }

		private IClient client;
		private readonly MessageHelper messageHelper;
		private IMessageHandler messageHandler;

		public GameClient()
		{
			messageHelper = new MessageHelper();
			ServerEntities = new List<ServerEntity>();
			messageHandler = ServiceLocator.Get<IMessageHandler>();
			Logger.RegisterLogLevelsFor<GameClient>(Logger.LogLevels.Adaptive);
		}

		public void Initialize(GameClientSettings settings)
		{
			client = new LidgrenClient();

			var configuration = new DefaultClientConfiguration
			{
				ServerAddress = settings.ServerAddress,
				ServerPort = settings.ServerPort
			};

			client.Initialize(configuration);
		}

		public void Connect()
		{
			client.Connect();
		}

		public void Disconnect(string message = null)
		{
			client.Disconnect(message);
		}

		public void Update(GameTime gameTime)
		{
			client.Update();
			messageHandler.Clear(MessageHandlerType.GameClient);

			for (int i = 0; i < client.Messages.Count; i++)
			{
				var message = client.Messages[i];

				switch ((PacketType)message.Data[0])
				{
					case PacketType.GameSettings:
						ReceivedGameSettings(message);
						break;

					case PacketType.EntityStatus:
						ReceivedClientStatus(message);
						break;

					case PacketType.EntitySpatial:
						ReceivedClientSpatial(message);
						break;

					case PacketType.EntityEvents:
						ReceivedEntityEvents(message);
						break;
				}

				// TODO: Temp Debug
				if ((PacketType)message.Data[0] != PacketType.EntitySpatial)
				{
					Logger.Log<GameClient>(LogLevel.Debug, "Received Data: {0} ({1} bytes)", messageHelper.BytesToString(message.Data), message.Data.Length);
				}
			}

			client.Messages.Clear();

			if (World != null)
			{
				// Update the physics simulation
				World.PhysicsHandler.Update(gameTime);

				// Update all world entities
				World.EntityHandler.Update(gameTime);
			}
		}

		private void UpdateServerEntities(ushort clientId, EntityType type, bool connected)
		{
			// Skip self
			if (ClientId == clientId)
			{
				return;
			}

			// Remove any previous instance of this entity if available
			RemoveServerGameEntityById(clientId);

			// Add the new entity
			if (connected)
			{
				var entity = new ServerEntity
				{
					Id = clientId,
					Type = type
				};

				ServerEntities.Add(entity);
			}
		}

		private void RemoveServerGameEntityById(ushort id)
		{
			for (int i = 0; i < ServerEntities.Count; i++)
			{
				if (ServerEntities[i].Id == id)
				{
					ServerEntities.Remove(ServerEntities[i]);
					break;
				}
			}
		}
	}
}