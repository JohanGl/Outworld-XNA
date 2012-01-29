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
		public event EventHandler<ClientSpatialEventArgs> GetClientSpatialCompleted;
		public event EventHandler<ClientActionsEventArgs> GetClientActionsCompleted;

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
				ClientId = clientId,
				Type = connected ? NetworkMessageType.Connected : NetworkMessageType.Disconnected,
				Text = string.Format("Player {0} {1}", clientId, connected ? "connected" : "disconnected")
			};

			messageHandler.AddMessage(MessageHandlerType.GameClient, notificationMessage);

			UpdateServerEntities(clientId, connected);
		}

		private void ReceivedClientSpatial(Message message)
		{
			if (GetClientSpatialCompleted != null)
			{
				var args = new ClientSpatialEventArgs();

				// Read the message
				client.Reader.ReadNewMessage(message);
				client.Reader.ReadByte();

				// Find out how many clients that are included in the message
				byte clients = client.Reader.ReadByte();

				// Initialize and retrieve all client spatial data
				args.Client = new ClientSpatial[clients];

				for (byte i = 0; i < clients; i++)
				{
					var data = new ClientSpatial();
					data.ClientId = client.Reader.ReadByte();
					data.TimeStamp = client.Reader.ReadTimeStamp();
					data.Position = messageHelper.ReadVector3(client.Reader);
					data.Velocity = messageHelper.ReadVector3(client.Reader);
					data.Angle = messageHelper.ReadVector3(client.Reader);

					args.Client[i] = data;
				}

				GetClientSpatialCompleted(this, args);
			}
		}

		private void ReceivedClientActions(Message message)
		{
			if (GetClientActionsCompleted != null)
			{
				var args = new ClientActionsEventArgs();

				// Read the message
				client.Reader.ReadNewMessage(message);
				client.Reader.ReadByte();

				// Get the number of clients in this message
				byte clients = client.Reader.ReadByte();

				for (byte i = 0; i < clients; i++)
				{
					// Get the id of the current client
					byte clientId = client.Reader.ReadByte();

					// Get the number of actions for the current client
					byte actions = client.Reader.ReadByte();

					for (byte j = 0; j < actions; j++)
					{
						var action = new ClientAction();
						action.ClientId = clientId;
						action.TimeStamp = client.Reader.ReadTimeStamp();
						action.Type = (ServerEntityEventType)client.Reader.ReadByte();

						args.ClientActions.Add(action);
					}
				}

				GetClientActionsCompleted(this, args);
			}
		}

		private void ReceivedSequence(Message message)
		{
			// Read the message
			client.Reader.ReadNewMessage(message);
			client.Reader.ReadByte();
			int sequenceCount = client.Reader.ReadByte();

			// Acknowledge all sequences within the message
			for (int i = 0; i < sequenceCount; i++)
			{
				byte currentSequence = client.Reader.ReadByte();
				unacknowledgedActions.Remove(currentSequence);
			}
		}
	}
}