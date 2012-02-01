using System;
using System.Collections.Generic;
using Framework.Core.Common;
using Framework.Core.Diagnostics.Logging;
using Framework.Network.Messages;
using Framework.Network.Servers;
using Framework.Network.Servers.Configurations;
using Game.Network.Common;
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
		private Dictionary<long, ushort> connectionIds;
		private Dictionary<ushort, EntityInfo> entities;
		private Dictionary<ushort, List<EntityEvent>> tempEntityEvents;
		private MessageHelper messageHelper;
		private GameTimer tickrateTimer;
		private GameTimer timeoutTimer;

		public GameServer()
		{
			messageHelper = new MessageHelper();
			entities = new Dictionary<ushort, EntityInfo>();
			connectionIds = new Dictionary<long, ushort>();
			tempEntityEvents = new Dictionary<ushort, List<EntityEvent>>();

			tickrateTimer = new GameTimer(TimeSpan.FromMilliseconds(1000 / 20));
			timeoutTimer = new GameTimer(TimeSpan.FromSeconds(20));

			InitializePacketSizeLookup();

			Logger.RegisterLogLevelsFor<GameServer>(Logger.LogLevels.Adaptive);
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

			entities.Clear();
		}

		public void Start()
		{
			World = new WorldSimulation();
			World.Initialize(Settings.World.Gravity, Settings.World.Seed);

			entities.Clear();
			server.Start();
		}

		public void Stop(string message = null)
		{
			server.Stop(message);
			entities.Clear();
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

			// Process all stored messages
			for (int i = 0; i < server.Messages.Count; i++)
			{
				ProcessMessage(gameTime, server.Messages[i]);
			}

			// Clear all messages to prepare for a new batch the next time this function is called
			server.Messages.Clear();

			var clients = GetEntitiesOfType(EntityType.Client);

			// Send updates to the clients
			BroadcastEntitySpatial(clients);
			BroadcastEntityEvents(clients);

			// Update the world
			World.Update(gameTime);
		}

		private void CheckTimeouts(GameTime gameTime)
		{
			var clientsToRemove = new List<ushort>();

			var clients = GetEntitiesOfType(EntityType.Client);

			foreach (var client in clients)
			{
				if (gameTime.TotalGameTime.Seconds - client.Timeout >= 20)
				{
					var message = new Message();
					message.ClientId = GetClientIdAsLong(client.Id).Value;
					message.Type = MessageType.Disconnect;
					server.Messages.Add(message);

					clientsToRemove.Add(client.Id);

					Logger.Log<LidgrenServer>(LogLevel.Debug, "Log: Client: {0} disconnected due to timeout.", client.Id.ToString());
				}
			}

			for (int i = 0; i < clientsToRemove.Count; i++)
			{
				entities.Remove(clientsToRemove[i]);
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
				if (entities.ContainsKey(clientId))
				{
					entities[clientId].Timeout = gameTime.TotalGameTime.Seconds;
				}

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
					string bytes = "";
					for (int j = 0; j < message.Data.Length; j++)
					{
						bytes += Convert.ToInt16(message.Data[j]).ToString();

						if (j < message.Data.Length - 1)
						{
							bytes += ",";
						}
					}

					Logger.Log<GameServer>(LogLevel.Debug, "Received Data: {0} ({1} bytes)", bytes, message.Data.Length);
				}
			}
			else
			{
				var args = new EntityStatusArgs { Type = EntityType.Client };

				if (message.Type == MessageType.Connect)
				{
					args.StatusType = EntityStatusType.Connected;
					args.Id = CreateClientIdAsShortMapping(message.ClientId);
					entities.Add(args.Id, new EntityInfo(args.Id));
				}
				else
				{
					args.StatusType = EntityStatusType.Disconnected;
					args.Id = connectionIds[message.ClientId];
					entities.Remove(args.Id);
					connectionIds.Remove(message.ClientId);
				}

				BroadcastClientStatusChanged(args);
			}
		}

		private ushort CreateClientIdAsShortMapping(long clientId)
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

		private long? GetClientIdAsLong(ushort clientId)
		{
			foreach (var connectionId in connectionIds)
			{
				if (connectionId.Value == clientId)
				{
					return connectionId.Key;
				}
			}

			return null;
		}

		private List<EntityInfo> GetEntitiesOfType(EntityType type)
		{
			var result = new List<EntityInfo>();

			for (ushort i = 0; i < entities.Count; i++)
			{
				if (entities[i].Type == type)
				{
					result.Add(entities[i]);
				}
			}

			return result;
		}
	}
}