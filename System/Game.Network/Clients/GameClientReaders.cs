using System;
using Framework.Network.Messages;
using Game.Entities;
using Game.Network.Clients.Events;
using Game.Network.Common;

namespace Game.Network.Clients
{
	public partial class GameClient
	{
		public event EventHandler<GameSettingsEventArgs> GetGameSettingsCompleted;
		public event EventHandler<ClientSpatialEventArgs> GetEntitySpatialCompleted;
		public event EventHandler<ClientEventsEventArgs> GetEntityEventsCompleted;

		private void ReceivedGameSettings(Message message)
		{
			if (GetGameSettingsCompleted != null)
			{
				var args = new GameSettingsEventArgs();

				// Read the message
				client.Reader.ReadNewMessage(message);
				client.Reader.ReadByte();
				args.ClientId = client.Reader.ReadUInt16();
				args.Seed = client.Reader.ReadInt32();

				// Assign the client id
				ClientId = args.ClientId;

				byte totalClients = client.Reader.ReadByte();

				for (int i = 0; i < totalClients; i++)
				{
					ushort currentClientId = client.Reader.ReadUInt16();
					UpdateServerEntities(currentClientId, EntityType.Client, true);
				}

				GetGameSettingsCompleted(this, args);
			}
		}

		private void ReceivedClientStatus(Message message)
		{
			// Read the message
			client.Reader.ReadNewMessage(message);
			client.Reader.ReadByte();
			ushort clientId = client.Reader.ReadUInt16();
			bool connected = client.Reader.ReadBool();

			// Add a global message for this event
			var notificationMessage = new NetworkMessage()
			{
				ClientId = clientId,
				Type = connected ? NetworkMessageType.Connected : NetworkMessageType.Disconnected,
				Text = string.Format("Player {0} {1}", clientId, connected ? "connected" : "disconnected")
			};

			messageHandler.AddMessage(MessageHandlerType.GameClient, notificationMessage);

			UpdateServerEntities(clientId, EntityType.Client, connected);
		}

		private void ReceivedClientSpatial(Message message)
		{
			if (GetEntitySpatialCompleted != null)
			{
				var args = new ClientSpatialEventArgs();

				// Read the message
				client.Reader.ReadNewMessage(message);
				client.Reader.ReadByte();

				// Find out how many clients that are included in the message
				byte clients = client.Reader.ReadByte();

				// Initialize and retrieve all client spatial data
				args.Entity = new EntitySpatial[clients];

				for (byte i = 0; i < clients; i++)
				{
					var data = new EntitySpatial();
					data.Id = client.Reader.ReadUInt16();
					data.TimeStamp = client.Reader.ReadTimeStamp();
					data.Position = messageHelper.ReadVector3(client.Reader);
					data.Velocity = messageHelper.ReadVector3(client.Reader);
					data.Angle = messageHelper.ReadVector3(client.Reader);

					args.Entity[i] = data;
				}

				GetEntitySpatialCompleted(this, args);
			}
		}

		private void ReceivedEntityEvents(Message message)
		{
			if (GetEntityEventsCompleted != null)
			{
				var args = new ClientEventsEventArgs();

				// Read the message
				client.Reader.ReadNewMessage(message);
				client.Reader.ReadByte();

				// Get the number of clients in this message
				byte clients = client.Reader.ReadByte();

				for (byte i = 0; i < clients; i++)
				{
					// Get the id of the current client
					ushort clientId = client.Reader.ReadUInt16();

					// Get the number of events for the current client
					byte events = client.Reader.ReadByte();

					for (byte j = 0; j < events; j++)
					{
						var entityEvent = new EntityEvent();
						entityEvent.Id = clientId;
						entityEvent.TimeStamp = client.Reader.ReadTimeStamp();
						entityEvent.Type = (EntityEventType)client.Reader.ReadByte();

						args.Events.Add(entityEvent);
					}
				}

				GetEntityEventsCompleted(this, args);
			}
		}
	}
}