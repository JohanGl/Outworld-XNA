using System.Collections.Generic;
using Framework.Network.Messages;
using Game.Network.Common;
using Microsoft.Xna.Framework;

namespace Game.Network.Servers
{
	public partial class GameServer
	{
		// Handled combined messages
		private bool isCombined;
		private bool isCombinedInitialized;

		private void BroadcastClientStatusChanged(EntityStatusArgs e)
		{
			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)PacketType.EntityStatus);
			server.Writer.Write(e.Id);
			server.Writer.Write(e.StatusType == EntityStatusType.Connected);

			server.Broadcast(MessageDeliveryMethod.ReliableUnordered, GetClientIdAsLong(e.Id));
		}

		private void SendGameSettings(Message message)
		{
			var clientId = connectionIds[message.ClientId];

			var clients = GetEntitiesOfType(EntityType.Client);

			server.Writer.WriteNewMessage();
			server.Writer.Write((byte)PacketType.GameSettings);
			server.Writer.Write(clientId);
			server.Writer.Write(Settings.World.Seed);
			messageHelper.WriteVector3(Settings.World.Gravity, server.Writer);
			server.Writer.Write((byte)(clients.Count - 1));

			foreach (var client in clients)
			{
				if (client.Id != clientId)
				{
					server.Writer.Write(client.Id);
				}
			}

			server.Send(message.ClientId, MessageDeliveryMethod.ReliableUnordered);
		}

		private void BroadcastClientSpatial(List<EntityInfo> clients)
		{
			if (clients.Count < 2)
			{
				return;
			}

			// Send individual packets to all clients
			for (ushort i = 0; i < clients.Count; i++)
			{
				SendClientSpatial(clients[i], clients);
			}
		}

		private void SendClientSpatial(EntityInfo client, List<EntityInfo> clients)
		{
			var otherClients = GetClientsWithinViewDistance(client, clients);

			// No other clients nearby
			if (otherClients.Count == 0)
			{
				return;
			}

			// Write the header
			InitializeMessageWriter();
			server.Writer.Write((byte)PacketType.EntitySpatial);

			// Total number of clients in the message
			server.Writer.Write((byte)otherClients.Count);

			// Send the spatial data of all other clients within view distance to the player
			for (int i = 0; i < otherClients.Count; i++)
			{
				server.Writer.Write(otherClients[i].Id);

				// Write the client spatial data
				var spatial = otherClients[i].CurrentSpatial;
				server.Writer.Write(spatial.TimeStamp);
				messageHelper.WriteVector3(spatial.Position, server.Writer);
				messageHelper.WriteVector3(spatial.Velocity, server.Writer);
				messageHelper.WriteVector3(spatial.Angle, server.Writer);
			}

			SendMessage(MessageDeliveryMethod.Unreliable);
		}

		private void BroadcastClientActions(List<EntityInfo> clients)
		{
			if (clients.Count < 2)
			{
				return;
			}

			// Send individual packets to all clients
			for (ushort i = 0; i < clients.Count; i++)
			{
				SendClientActions(clients[i], clients);
			}
		}

		private void SendClientActions(EntityInfo client, List<EntityInfo> clients)
		{
			var otherClients = GetClientsWithinViewDistance(client, clients);

			// No other clients nearby
			if (otherClients.Count == 0)
			{
				return;
			}

			tempActions.Clear();

			for (int i = 0; i < otherClients.Count; i++)
			{
				var actions = otherClients[i].GetRecentActions(server.TimeStamp);

				if (actions.Count > 0)
				{
					tempActions.Add(otherClients[i].Id, actions);
				}
			}

			// No relevant actions for other clients
			if (tempActions.Count == 0)
			{
				return;
			}

			// Write the header
			InitializeMessageWriter();
			server.Writer.Write((byte)PacketType.EntityEvents);
			server.Writer.Write((ushort)tempActions.Count);

			// Loop through all entities and their actions
			foreach (var pair in tempActions)
			{
				// Write the current entity id
				server.Writer.Write(pair.Key);

				// Write the current entity actions
				for (int i = 0; i < pair.Value.Count; i++)
				{
					var action = pair.Value[i];

					server.Writer.Write(action.TimeStamp);
					server.Writer.Write((byte)action.Type);
				}
			}

			SendMessage(MessageDeliveryMethod.ReliableUnordered);
		}

		private List<EntityInfo> GetClientsWithinViewDistance(EntityInfo client, List<EntityInfo> clients)
		{
			var otherClients = new List<EntityInfo>();

			for (ushort i = 0; i < clients.Count; i++)
			{
				var otherClient = clients[i];

				if (otherClient.Id != client.Id)
				{
					if (otherClient.IsWithinViewDistanceOf(client))
					{
						otherClients.Add(otherClient);
					}
				}
			}

			return otherClients;
		}

		private void InitializeMessageWriter()
		{
			if (!isCombined)
			{
				server.Writer.WriteNewMessage();
			}
			else if (isCombined && !isCombinedInitialized)
			{
				server.Writer.WriteNewMessage();
				server.Writer.Write((byte)PacketType.Combined);
				isCombinedInitialized = true;
			}
		}

		private void SendMessage(MessageDeliveryMethod deliveryMethod)
		{
			if (!isCombined)
			{
				server.Broadcast(deliveryMethod);
			}
		}

		public void BeginCombinedMessage()
		{
			isCombined = true;
			isCombinedInitialized = false;
		}

		public void EndCombinedMessage(MessageDeliveryMethod deliveryMethod)
		{
			server.Broadcast(deliveryMethod);
			isCombined = false;
		}
	}
}