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
		private Dictionary<byte, ClientData> clientData;
		private double timer;

		private GameTimer clientTimeOutTimer;

		public GameServer()
		{
			messageHelper = new MessageHelper();
			clientData = new Dictionary<byte, ClientData>();
			connectionIds = new Dictionary<long, byte>();

			clientTimeOutTimer = new GameTimer(TimeSpan.FromSeconds(2));

			Logger.RegisterLogLevelsFor<GameServer>(Logger.LogLevels.Adaptive);
		}

		private void CheckClientTimeOuts(GameTime gameTime)
		{
			var clientsToRemove = new List<byte>();

			foreach (var pair in clientData)
			{
				if (gameTime.TotalGameTime.Seconds - pair.Value.TimeOut >= 2)
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
				clientData.Remove(clientsToRemove[i]);
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

			clientData.Clear();
		}

		public void Start()
		{
			World = new WorldSimulation();
			World.Initialize(Settings.World.Gravity, Settings.World.Seed);

			clientData.Clear();
			server.Start();
		}

		public void Stop(string message = null)
		{
			server.Stop(message);
			clientData.Clear();
		}

		public void Update(GameTime gameTime)
		{
			server.Update();

			for (int i = 0; i < server.Messages.Count; i++)
			{
				Message message = server.Messages[i];

				if (message.Type == MessageType.Data)
				{
					// Skip clients that doesnt exist
					if (!connectionIds.ContainsKey(message.ClientId))
					{
						continue;
					}

					// Reset client timeout value when data recieved
					byte clientId = connectionIds[message.ClientId];
					if (clientData.ContainsKey(clientId))
					{
						clientData[clientId].TimeOut = gameTime.TotalGameTime.Seconds;
					}

					switch ((GameClientMessageType)message.Data[0])
					{
						case GameClientMessageType.GameSettings:
							SendGameSettings(message);
							break;

						case GameClientMessageType.ClientSpatial:
							ReceivedClientSpatial(message);
							break;
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
						clientData.Add(args.ClientId, new ClientData());
					}
					else
					{
						args.ClientId = connectionIds[message.ClientId];
						clientData.Remove(args.ClientId);
						connectionIds.Remove(message.ClientId);
					}

					BroadcastClientStatusChanged(args);
				}
			}

			server.Messages.Clear();

			if (server.IsStarted)
			{
				if (gameTime.TotalGameTime.TotalMilliseconds > timer)
				{
					BroadcastClientSpatial();

					timer = gameTime.TotalGameTime.TotalMilliseconds + 33;
				}

				World.Update(gameTime);
			}

			if(clientTimeOutTimer.Update(gameTime))
			{
				CheckClientTimeOuts(gameTime);
			}
		}

		private void BroadcastClientStatusChanged(ClientStatusArgs e)
		{
			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)GameClientMessageType.ClientStatus);
			server.Writer.Write(e.ClientId);
			server.Writer.Write(e.Type == ClientStatusType.Connected);

			server.Broadcast(MessageDeliveryMethod.ReliableUnordered, GetClientIdAsLong(e.ClientId));
		}

		private void SendGameSettings(Message message)
		{
			byte clientId = connectionIds[message.ClientId];

			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)GameClientMessageType.GameSettings);

			server.Writer.Write(clientId);
			server.Writer.Write(Settings.World.Seed);
			messageHelper.WriteVector3(Settings.World.Gravity, server.Writer);
			server.Writer.Write((byte)clientData.Count - 1);

			foreach (var pair in clientData)
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
			server.Reader.ReadNewMessage(message);
			server.Reader.ReadByte();

			var clientSpatialData = new ClientSpatialData
			{
				ClientId = connectionIds[message.ClientId],
				Position = messageHelper.ReadVector3(server.Reader),
				Velocity = messageHelper.ReadVector3(server.Reader),
				Angle = messageHelper.ReadVector3(server.Reader)
			};

			if (!clientData.ContainsKey(clientSpatialData.ClientId))
			{
				return;
			}

			clientData[clientSpatialData.ClientId].SpatialData.Add(clientSpatialData);

			// Keep a maximum of 2 entries
			if (clientData[clientSpatialData.ClientId].SpatialData.Count > 2)
			{
				clientData[clientSpatialData.ClientId].SpatialData.RemoveAt(0);
			}
		}

		private void BroadcastClientSpatial()
		{
			if (clientData.Count < 2)
			{
				return;
			}

			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)GameClientMessageType.ClientSpatial);

			// Total number of clients in the message
			server.Writer.Write((byte)clientData.Count);

			// Write all client positions
			foreach (var pair in clientData)
			{
				int length = pair.Value.SpatialData.Count - 1;

				server.Writer.Write(pair.Key);

				if (length >= 0)
				{
					messageHelper.WriteVector3(pair.Value.SpatialData[length].Position, server.Writer);
					messageHelper.WriteVector3(pair.Value.SpatialData[length].Velocity, server.Writer);
					messageHelper.WriteVector3(pair.Value.SpatialData[length].Angle, server.Writer);
				}
				else
				{
					messageHelper.WriteVector3(Vector3.Zero, server.Writer);
					messageHelper.WriteVector3(Vector3.Zero, server.Writer);
					messageHelper.WriteVector3(Vector3.Zero, server.Writer);
				}
			}

			server.Broadcast(MessageDeliveryMethod.UnreliableSequenced);
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