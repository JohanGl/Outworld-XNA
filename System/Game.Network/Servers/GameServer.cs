using System.Collections.Generic;
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
		private MessageHelper messageHelper;
		private Dictionary<byte, ClientData> clientData;
		private double timer;

		public GameServer()
		{
			messageHelper = new MessageHelper();
			clientData = new Dictionary<byte, ClientData>();
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
				if (server.Messages[i].Type == MessageType.Data)
				{
					switch ((GameClientMessageType)server.Messages[i].Data[0])
					{
						case GameClientMessageType.GameSettings:
							SendGameSettings(server.Messages[i]);
							break;

						case GameClientMessageType.ClientSpatial:
							ReceivedClientSpatial(server.Messages[i]);
							break;
					}
				}
				else
				{
					var args = new ClientStatusArgs()
					{
						ClientId = server.GetClientIdAsByte(server.Messages[i].ClientId),
						Type = server.Messages[i].Type == MessageType.Connect ? ClientStatusType.Connected : ClientStatusType.Disconnected
					};

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
		}

		private void BroadcastClientStatusChanged(ClientStatusArgs e)
		{
			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)GameClientMessageType.ClientStatus);
			server.Writer.Write(e.ClientId);
			server.Writer.Write(e.Type == ClientStatusType.Connected);
//			server.Writer.Write(server.GetClientIdAsLong(e.ClientId));

			long? clientId = server.GetClientIdAsLong(e.ClientId);

			if (clientId == -1)
			{
				clientId = null;
			}

			server.Broadcast(MessageDeliveryMethod.ReliableUnordered, clientId);
		}

		private void SendGameSettings(Message message)
		{
			byte clientId = server.GetClientIdAsByte(message.ClientId);

			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)GameClientMessageType.GameSettings);

			server.Writer.Write(clientId);
			server.Writer.Write(Settings.World.Seed);
			messageHelper.WriteVector3(Settings.World.Gravity, server.Writer);
			server.Writer.Write((byte)clientData.Count);

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
				ClientId = server.GetClientIdAsByte(message.ClientId),
				Position = messageHelper.ReadVector3(server.Reader),
				Velocity = messageHelper.ReadVector3(server.Reader),
				Angle = messageHelper.ReadVector3(server.Reader)
			};

			if (!clientData.ContainsKey(clientSpatialData.ClientId))
			{
				clientData.Add(clientSpatialData.ClientId, new ClientData());
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
	}
}