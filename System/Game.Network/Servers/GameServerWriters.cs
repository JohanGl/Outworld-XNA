using Framework.Network.Messages;
using Framework.Network.Servers;
using Game.Network.Common;
using Microsoft.Xna.Framework;

namespace Game.Network.Servers
{
	public partial class GameServer
	{
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
			server.Writer.Write((byte)(clients.Count - 1));

			foreach (var pair in clients)
			{
				if (pair.Key != clientId)
				{
					server.Writer.Write(pair.Key);
				}
			}

			server.Send(message.ClientId, MessageDeliveryMethod.ReliableOrdered);
		}

		// TODO: Optimize broadcast
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
					messageHelper.WriteVector3(client.Value.SpatialData[length].Angle, server.Writer);
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
	}
}