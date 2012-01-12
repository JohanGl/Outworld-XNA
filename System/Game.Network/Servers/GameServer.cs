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
	public class GameServer : IGameServer
	{
		public GameServerSettings Settings { get; private set; }
		public WorldSimulation World { get; private set; }

		private IServer server;
		private Dictionary<long, byte> connectionIds;
		private MessageHelper messageHelper;
		private Dictionary<byte, ClientData> clients;
		private GameTimer tickrateTimer;
		private GameTimer checkClientTimeoutsTimer;

		private Dictionary<PacketType, int> packetSizeLookup;
		private int combinedMessageStartIndex;

		public GameServer()
		{
			messageHelper = new MessageHelper();
			clients = new Dictionary<byte, ClientData>();
			connectionIds = new Dictionary<long, byte>();

			tickrateTimer = new GameTimer(TimeSpan.FromMilliseconds(1000 / 30));
			checkClientTimeoutsTimer = new GameTimer(TimeSpan.FromSeconds(20));

			packetSizeLookup = new Dictionary<PacketType, int>();
			packetSizeLookup.Add(PacketType.ClientSpatial, 28);
			packetSizeLookup.Add(PacketType.ClientActions, 0);

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
						SendGameSettings(message);
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

		private void BroadcastClientStatusChanged(ClientStatusArgs e)
		{
			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)PacketType.ClientStatus);
			server.Writer.Write(e.ClientId);
			server.Writer.Write(e.Type == ClientStatusType.Connected);

			server.Broadcast(MessageDeliveryMethod.ReliableUnordered, GetClientIdAsLong(e.ClientId));
		}

		private void SendGameSettings(Message message)
		{
			byte clientId = connectionIds[message.ClientId];

			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)PacketType.GameSettings);

			server.Writer.Write(clientId);
			server.Writer.Write(Settings.World.Seed);
			messageHelper.WriteVector3(Settings.World.Gravity, server.Writer);
			server.Writer.Write((byte)clients.Count - 1);

			foreach (var pair in clients)
			{
				if (pair.Key != clientId)
				{
					server.Writer.Write(pair.Key);
				}
			}

			server.Send(message.ClientId, MessageDeliveryMethod.ReliableOrdered);
		}

		private void ReceivedClientSpatial(Message message)
		{
			byte clientId = connectionIds[message.ClientId];

			if (!clients.ContainsKey(clientId))
			{
				return;
			}

			server.Reader.ReadNewMessage(message);
			server.Reader.ReadByte();

			var clientSpatialData = new ClientSpatialData();
			clientSpatialData.ClientId = clientId;
			clientSpatialData.Position = messageHelper.ReadVector3(server.Reader);
			clientSpatialData.Velocity = messageHelper.ReadVector3(server.Reader);
			clientSpatialData.Angle = messageHelper.ReadVector3FromVector3b(server.Reader);
			clientSpatialData.Time = DateTime.UtcNow;

			clients[clientSpatialData.ClientId].SpatialData.Add(clientSpatialData);

			// Keep a maximum of 2 entries
			if (clients[clientSpatialData.ClientId].SpatialData.Count > 2)
			{
				clients[clientSpatialData.ClientId].SpatialData.RemoveAt(0);
			}
		}

		private void ReceivedClientActions(Message message)
		{
			byte clientId = connectionIds[message.ClientId];

			if (!clients.ContainsKey(clientId))
			{
				return;
			}

			server.Reader.ReadNewMessage(message);
			server.Reader.ReadByte();

			int actions = server.Reader.ReadByte();

			for (int i = 0; i < actions; i++)
			{
				var action = new ClientAction()
				{
					Time = DateTime.UtcNow,
					Type = (ClientActionType)server.Reader.ReadByte()
				};

				clients[clientId].Actions.Add(action);

				// Keep a maximum of 10 entries per server tickRate update
				if (clients[clientId].Actions.Count > 10)
				{
					clients[clientId].Actions.RemoveAt(0);
				}
			}
		}

		private void ReceivedCombinedMessage(Message message)
		{
			byte clientId = connectionIds[message.ClientId];

			if (!clients.ContainsKey(clientId))
			{
				return;
			}

			combinedMessageStartIndex = 1;

			while (HasMoreMessagesInCombinedMessage(message))
			{
				server.Messages.Add(GetNextMessageInCombinedMessage(message));
			}
		}

		private bool HasMoreMessagesInCombinedMessage(Message message)
		{
			return (combinedMessageStartIndex < message.Data.Length);
		}

		private Message GetNextMessageInCombinedMessage(Message message)
		{
			var type = (PacketType)message.Data[combinedMessageStartIndex];
			var size = packetSizeLookup[type];

			// Dynamic size packet?
			if (size == 0)
			{
				// The second byte represents the length of the packet (+ 2 for the header and length bytes)
				size = message.Data[combinedMessageStartIndex + 1] + 2;
			}

			var result = new Message
			{
				ClientId = message.ClientId,
				Type = MessageType.Data,
				Data = new byte[size]
			};

			Array.Copy(message.Data, combinedMessageStartIndex, result.Data, 0, size);
			combinedMessageStartIndex += size;

			return result;
		}

		private void BroadcastClientSpatial()
		{
			if (clients.Count < 2)
			{
				return;
			}

			// Write the header
			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)PacketType.ClientSpatial);

			// Total number of clients in the message
			server.Writer.Write((byte)clients.Count);

			// Write all client positions
			foreach (var client in clients)
			{
				int length = client.Value.SpatialData.Count - 1;

				// Write the current client id
				server.Writer.Write(client.Key);

				if (length >= 0)
				{
					// Write the client spatial data
					messageHelper.WriteVector3(client.Value.SpatialData[length].Position, server.Writer);
					messageHelper.WriteVector3(client.Value.SpatialData[length].Velocity, server.Writer);
					messageHelper.WriteVector3AsVector3b(client.Value.SpatialData[length].Angle, server.Writer);
				}
				else
				{
					messageHelper.WriteVector3(Vector3.Zero, server.Writer);
					messageHelper.WriteVector3(Vector3.Zero, server.Writer);
					messageHelper.WriteVector3AsVector3b(Vector3.Zero, server.Writer);
				}
			}

			server.Broadcast(MessageDeliveryMethod.UnreliableSequenced);
		}

		private void BroadcastClientActions()
		{
			if (clients.Count < 2)
			{
				return;
			}

			int clientsWithActions = 0;

			foreach (var client in clients.Values)
			{
				if (client.Actions.Count > 0)
				{
					clientsWithActions++;
				}
			}

			if (clientsWithActions == 0)
			{
				return;
			}

			bool hasDataToSend = false;

			// Write the header
			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)PacketType.ClientActions);
			server.Writer.Write((byte)clientsWithActions);

			foreach (var client in clients)
			{
				var clientData = client.Value;

				if (clientData.Actions.Count == 0)
				{
					continue;
				}

				hasDataToSend = true;

				// Write the current client id and number of actions
				server.Writer.Write(client.Key);
				server.Writer.Write((byte)clientData.Actions.Count);

				for (int i = 0; i < clientData.Actions.Count; i++)
				{
					var action = clientData.Actions[i];

					// Write the current client action
					server.Writer.Write((byte)action.Type);
				}

				clientData.Actions.Clear();
			}

			if (hasDataToSend)
			{
				server.Broadcast(MessageDeliveryMethod.ReliableSequenced);
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