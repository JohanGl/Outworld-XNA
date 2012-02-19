using System;
using System.Collections.Generic;
using Framework.Core.Common;
using Framework.Core.Diagnostics.Logging;
using Framework.Network.Messages;
using Framework.Network.Servers;
using Framework.Network.Servers.Configurations;
using Game.Network.Common;
using Game.Network.Servers.Helpers;
using Game.Network.Servers.Settings;
using Game.Network.Servers.Simulations.World;
using Microsoft.Xna.Framework;

namespace Game.Network.Servers
{
	public partial class GameServer : IGameServer
	{
		public GameServerSettings Settings { get; private set; }
		public WorldSimulation World { get; private set; }

		private IServer server;
		private IMessageRecorder recorder;
		private ServerEntityHelper entityHelper;
		private Dictionary<ushort, List<EntityEvent>> tempEntityEvents;

		private Dictionary<long, ushort> connectionIds;
		private MessageHelper messageHelper;
		private GameTimer tickrateTimer;
		private GameTimer timeoutTimer;

		public GameServer()
		{
			messageHelper = new MessageHelper();
			connectionIds = new Dictionary<long, ushort>();

			entityHelper = new ServerEntityHelper();
			tempEntityEvents = new Dictionary<ushort, List<EntityEvent>>();

			tickrateTimer = new GameTimer(TimeSpan.FromMilliseconds(1000 / 20));
			timeoutTimer = new GameTimer(TimeSpan.FromSeconds(20));

			InitializePacketSizeLookup();

			Logger.RegisterLogLevelsFor<GameServer>(Logger.LogLevels.Adaptive);

#if DEBUG
			recorder = new DefaultMessageRecorder();
#endif
		}

		public bool IsStarted
		{
			get
			{
				return server.IsStarted;
			}
		}

		public void Initialize(GameServerSettings settings)
		{
			Settings = settings;

			var configuration = new DefaultServerConfiguration
			{
				Port = settings.Port,
				MaximumConnections = settings.MaximumConnections
			};

			server = new LidgrenServer();
			server.Initialize(configuration);

			entityHelper.Clear();
			tempEntityEvents.Clear();
		}

		public void Start()
		{
			World = new WorldSimulation();
			World.Initialize(Settings.World.Gravity, Settings.World.Seed);

			entityHelper.Clear();
			tempEntityEvents.Clear();
			server.Start();
		}

		public void Stop(string message = null)
		{
			server.Stop(message);
			entityHelper.Clear();
			tempEntityEvents.Clear();
		}

		public void Update(GameTime gameTime)
		{
			if (!server.IsStarted)
			{
				return;
			}

			// Check for and remove clients that have timed out
			if (timeoutTimer.Update(gameTime))
			{
				CheckTimeouts(gameTime);
			}

			// Skip the packet updates below if the tickrate hasnt been reached
			if (!tickrateTimer.Update(gameTime))
			{
				return;
			}

			// Check for new messages and store them if available
			server.Update();

#if DEBUG
			recorder.Record(server.Messages);
#endif

			// Process all stored messages
			for (int i = 0; i < server.Messages.Count; i++)
			{
				ProcessMessage(gameTime, server.Messages[i]);
			}

			// Clear all messages to prepare for a new batch the next time this function is called
			server.Messages.Clear();

			var clients = entityHelper.GetEntitiesOfType(EntityType.Client);

			// Send updates to the clients
			BroadcastEntitySpatial(clients);
			BroadcastEntityEvents(clients);

			// Update the world
			World.Update(gameTime);
		}

		private void CheckTimeouts(GameTime gameTime)
		{
			var clientsToRemove = new List<ushort>();

			var clients = entityHelper.GetEntitiesOfType(EntityType.Client);

			foreach (var client in clients)
			{
				if (gameTime.TotalGameTime.Seconds - client.Timeout >= 20)
				{
					var message = new Message();
					message.ClientId = client.ServerId;
					message.Type = MessageType.Disconnect;
					server.Messages.Add(message);

					clientsToRemove.Add(client.Id);

					Logger.Log<LidgrenServer>(LogLevel.Debug, "Log: Client: {0} disconnected due to timeout.", client.Id.ToString());
				}
			}

			for (int i = 0; i < clientsToRemove.Count; i++)
			{
				entityHelper.Entities.Remove(clientsToRemove[i]);
			}
		}

		private void ProcessMessage(GameTime gameTime, Message message)
		{
			if (message.Type == MessageType.Data)
			{
				// Skip clients that doesnt exist
				if (!connectionIds.ContainsKey(message.ClientId))
				{
					return;
				}

				// Reset the client timeout counter every time data has been received
				var clientId = connectionIds[message.ClientId];

				entityHelper.UpdateTimeout(gameTime, clientId);

				switch ((PacketType)message.Data[0])
				{
					case PacketType.Combined:
						ReceivedCombinedMessage(message);
						break;

					case PacketType.GameSettings:
						ReceivedGameSettingsRequest(message);
						break;

					case PacketType.EntitySpatial:
						ReceivedEntitySpatial(message);
						break;

					case PacketType.EntityEvents:
						ReceivedEntityEvents(message);
						break;
				}

				// TODO: Temp Debug
				if ((PacketType)message.Data[0] != PacketType.EntitySpatial)
				{
					Logger.Log<GameServer>(LogLevel.Debug, "Received Data: {0} ({1} bytes)", messageHelper.BytesToString(message.Data), message.Data.Length);
				}
			}
			else
			{
				var status = new EntityStatusArgs
				{
					Type = EntityType.Client,
					ServerId = message.ClientId
				};

				if (message.Type == MessageType.Connect)
				{
					status.StatusType = EntityStatusType.Connected;
					status.Id = CreateShortClientId(message.ClientId);

					entityHelper.Entities.Add(status.Id, CreateConnectedClient(status.Id, message));

					Logger.Log<GameServer>(LogLevel.Debug, "Client with id {0} connected with remote time offset: {1}", status.Id, message.RemoteTimeOffset);
				}
				else
				{
					status.StatusType = EntityStatusType.Disconnected;
					status.Id = connectionIds[message.ClientId];

					entityHelper.Entities.Remove(status.Id);
					connectionIds.Remove(message.ClientId);

					Logger.Log<GameServer>(LogLevel.Debug, "Client with id {0} disconnected", status.Id);
				}

				BroadcastClientStatusChanged(status);
			}
		}

		private ushort CreateShortClientId(long clientId)
		{
			// Initialize the client id if not already done
			if (!connectionIds.ContainsKey(clientId))
			{
				ushort newId = 0;

				// Find a unused id
				for (int i = 0; i < ushort.MaxValue; i++)
				{
					bool foundNewId = true;

					foreach (var pair in connectionIds)
					{
						if (pair.Value == i)
						{
							foundNewId = false;
							break;
						}
					}

					if (foundNewId)
					{
						newId = (ushort)i;
						break;
					}
				}

				// Add the new client id to the lookup table
				connectionIds.Add(clientId, newId);
			}

			return connectionIds[clientId];
		}

		private ServerEntity CreateConnectedClient(ushort id, Message message)
		{
			var entityInfo = new ServerEntity(id, message.ClientId);
			entityInfo.RemoteTimeOffset = message.RemoteTimeOffset;
			return entityInfo;
		}
	}
}