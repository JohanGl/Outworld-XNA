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
		private Dictionary<long, byte> connectionIds;
		private MessageHelper messageHelper;
		private Dictionary<byte, ClientData> clients;
		private GameTimer tickrateTimer;
		private GameTimer checkClientTimeoutsTimer;

		public GameServer()
		{
			messageHelper = new MessageHelper();
			clients = new Dictionary<byte, ClientData>();
			connectionIds = new Dictionary<long, byte>();

			tickrateTimer = new GameTimer(TimeSpan.FromMilliseconds(1000 / 20));
			checkClientTimeoutsTimer = new GameTimer(TimeSpan.FromSeconds(20));

			InitializePacketSizeLookup();

			Logger.RegisterLogLevelsFor<GameServer>(Logger.LogLevels.Adaptive);
		}

		private void CheckClientTimeouts(GameTime gameTime)
		{
			var clientsToRemove = new List<byte>();

			foreach (var pair in clients)
			{
				if (gameTime.TotalGameTime.Seconds - pair.Value.Timeout >= 20)
				{
					var message = new Message();
					message.ClientId = GetClientIdAsLong(pair.Key).Value;
					message.Type = MessageType.Disconnect;
					server.Messages.Add(message);

					clientsToRemove.Add(pair.Key);

					Logger.Log<LidgrenServer>(LogLevel.Debug, "Log: Client:{0} Disconnected due to TimeOut.", pair.Key.ToString());
				}
			}

			for (int i = 0; i < clientsToRemove.Count; i++)
			{
				clients.Remove(clientsToRemove[i]);
			}
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

			clients.Clear();
		}

		public void Start()
		{
			World = new WorldSimulation();
			World.Initialize(Settings.World.Gravity, Settings.World.Seed);

			clients.Clear();
			server.Start();
		}

		public void Stop(string message = null)
		{
			server.Stop(message);
			clients.Clear();
		}

		public void Update(GameTime gameTime)
		{
			if (!server.IsStarted)
			{
				return;
			}

			// Check for and remove clients that have timed out
			if (checkClientTimeoutsTimer.Update(gameTime))
			{
				CheckClientTimeouts(gameTime);
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

			// Send updates to the clients
			BroadcastClientSpatial();
			BroadcastClientActions();

			// Update the world
			World.Update(gameTime);
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
				byte clientId = connectionIds[message.ClientId];
				if (clients.ContainsKey(clientId))
				{
					clients[clientId].Timeout = gameTime.TotalGameTime.Seconds;
				}

				switch ((PacketType)message.Data[0])
				{
					case PacketType.GameSettings:
						ReceivedGameSettingsRequest(message);
						break;

					case PacketType.ClientSpatial:
						ReceivedClientSpatial(message);
						break;

					case PacketType.ClientActions:
						ReceivedClientActions(message);
						break;

					case PacketType.Combined:
						ReceivedCombinedMessage(message);
						break;
				}

				// TODO: Temp Debug
				if ((PacketType)message.Data[0] != PacketType.ClientSpatial)
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
				var args = new ClientStatusArgs()
				{
					Type = message.Type == MessageType.Connect ? ClientStatusType.Connected : ClientStatusType.Disconnected
				};

				if (args.Type == ClientStatusType.Connected)
				{
					args.ClientId = CreateClientIdAsByteMapping(message.ClientId);
					clients.Add(args.ClientId, new ClientData());
				}
				else
				{
					args.ClientId = connectionIds[message.ClientId];
					clients.Remove(args.ClientId);
					connectionIds.Remove(message.ClientId);
				}

				BroadcastClientStatusChanged(args);
			}
		}

		private byte CreateClientIdAsByteMapping(long clientId)
		{
			// Initialize the client id if not already done
			if (!connectionIds.ContainsKey(clientId))
			{
				byte clientByteId = 0;

				// Find a unused id in the range 1-255
				for (int i = 1; i <= 255; i++)
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
						clientByteId = (byte)i;
						break;
					}
				}

				// Add the new client id to the lookup table
				connectionIds.Add(clientId, clientByteId);
			}

			return connectionIds[clientId];
		}

		private long? GetClientIdAsLong(byte clientId)
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
	}
}