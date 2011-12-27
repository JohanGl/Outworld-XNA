using System;
using System.Collections.Generic;
using Framework.Core.Diagnostics.Logging;
using Framework.Core.Messaging;
using Framework.Core.Services;
using Framework.Network.Clients;
using Framework.Network.Clients.Configurations;
using Framework.Network.Messages;
using Game.Network.Clients.Events;
using Game.Network.Clients.Settings;
using Game.Network.Common;
using Game.World;
using Microsoft.Xna.Framework;

namespace Game.Network.Clients
{
	public class GameClient : IGameClient
	{
		public event EventHandler<GameSettingsEventArgs> GetGameSettingsCompleted;
		public event EventHandler<ClientSpatialEventArgs> GetClientSpatialCompleted;
		public WorldContext World { get; set; }
		public byte ClientId { get; set; }
		public List<ServerEntity> ServerEntities { get; set; }

		public bool IsConnected
		{
			get
			{
				return client.IsConnected;
			}
		}

		private IClient client;
		private readonly MessageHelper messageHelper;
		private IMessageHandler messageHandler;

		// Handled combined messages
		private bool isCombined;
		private bool isCombinedInitialized;

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
			messageHandler.Clear("GameClient");

			for (int i = 0; i < client.Messages.Count; i++)
			{
				var message = client.Messages[i];

				switch ((PacketType)message.Data[0])
				{
					case PacketType.ClientStatus:
						ReceivedClientStatus(message);
						break;

					case PacketType.GameSettings:
						ReceivedGameSettings(message);
						break;

					case PacketType.ClientSpatial:
						ReceivedClientSpatial(message);
						break;
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

		private void ReceivedClientStatus(Message message)
		{
			// Read the message
			client.Reader.ReadNewMessage(message);
			client.Reader.ReadByte();
			byte clientId = client.Reader.ReadByte();
			bool connected = client.Reader.ReadBool();

			// Add a global message for this event
			var notificationMessage = new NetworkMessage()
			{
				Type = connected ? NetworkMessage.MessageType.Connected : NetworkMessage.MessageType.Disconnected,
				Text = string.Format("Player {0} {1}", clientId, connected ? "connected" : "disconnected")
			};

			messageHandler.AddMessage("GameClient", notificationMessage);

			UpdateServerEntities(clientId, connected);
		}

		private void UpdateServerEntities(byte clientId, bool connected)
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
					Id = clientId
				};

				ServerEntities.Add(entity);
			}
		}

		private void RemoveServerGameEntityById(byte id)
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

		public void GetGameSettings()
		{
			client.Writer.WriteNewMessage();
			client.Writer.Write((byte)PacketType.GameSettings);
			client.Send(MessageDeliveryMethod.ReliableUnordered);
		}

		private void ReceivedGameSettings(Message message)
		{
			if (GetGameSettingsCompleted != null)
			{
				var args = new GameSettingsEventArgs();

				// Read the message
				client.Reader.ReadNewMessage(message);
				client.Reader.ReadByte();
				args.ClientId = client.Reader.ReadByte();
				args.Seed = client.Reader.ReadInt32();
				args.Gravity = messageHelper.ReadVector3(client.Reader);

				// Assign the client id
				ClientId = args.ClientId;

				byte totalClients = client.Reader.ReadByte();

				for (int i = 0; i < totalClients; i++)
				{
					byte currentClientId = client.Reader.ReadByte();
					UpdateServerEntities(currentClientId, true);
				}

				GetGameSettingsCompleted(this, args);
			}
		}

		public void GetClientSpatial()
		{
			client.Writer.WriteNewMessage();
			client.Writer.Write((byte)PacketType.ClientSpatial);
			client.Send(MessageDeliveryMethod.UnreliableSequenced);
		}

		private void ReceivedClientSpatial(Message message)
		{
			if (GetClientSpatialCompleted != null)
			{
				var args = new ClientSpatialEventArgs();

				// Read the message
				client.Reader.ReadNewMessage(message);
				client.Reader.ReadByte();

				// Find out how many clients are included in the message
				byte clients = client.Reader.ReadByte();

				// Initialize and retrieve all client spatial data
				args.ClientData = new ClientSpatialData[clients];

				for (int i = 0; i < clients; i++)
				{
					var data = new ClientSpatialData();
					data.ClientId = client.Reader.ReadByte();
					data.Position = messageHelper.ReadVector3(client.Reader);
					data.Velocity = messageHelper.ReadVector3(client.Reader);
					data.Angle = messageHelper.ReadVector3FromVector3b(client.Reader);

					args.ClientData[i] = data;
				}

				GetClientSpatialCompleted(this, args);
			}
		}

		public void SendClientSpatial(Vector3 position, Vector3 velocity, Vector3 angle)
		{
			InitializeMessageWriter();
			client.Writer.Write((byte)PacketType.ClientSpatial);
			messageHelper.WriteVector3(position, client.Writer);
			messageHelper.WriteVector3(velocity, client.Writer);
			messageHelper.WriteVector3AsVector3b(angle, client.Writer);
			SendMessage();
		}

		public void SendClientActions(List<ClientAction> actions)
		{
			InitializeMessageWriter();
			client.Writer.Write((byte)PacketType.ClientActions);
			client.Writer.Write((byte)actions.Count);

			for (int i = 0; i < actions.Count; i++)
			{
				client.Writer.Write((byte)actions[i].Type);
			}

			SendMessage();
		}

		private void InitializeMessageWriter()
		{
			if (!isCombined)
			{
				client.Writer.WriteNewMessage();
			}
			else if (isCombined && !isCombinedInitialized)
			{
				client.Writer.WriteNewMessage();
				client.Writer.Write((byte)PacketType.Combined);
				isCombinedInitialized = true;
			}
		}

		private void SendMessage()
		{
			if (!isCombined)
			{
				client.Send(MessageDeliveryMethod.Unreliable);
			}
		}

		public void BeginCombinedMessage()
		{
			isCombined = true;
			isCombinedInitialized = false;
		}

		public void EndCombinedMessage()
		{
			client.Send(MessageDeliveryMethod.Unreliable);
			isCombined = false;
		}
	}
}