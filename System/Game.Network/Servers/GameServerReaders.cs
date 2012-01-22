using System;
using System.Collections.Generic;
using Framework.Network.Messages;
using Game.Network.Common;

namespace Game.Network.Servers
{
	public partial class GameServer
	{
		private Dictionary<PacketType, int> packetSizeLookup;
		private int combinedMessageStartIndex;

		private void InitializePacketSizeLookup()
		{
			packetSizeLookup = new Dictionary<PacketType, int>();
			packetSizeLookup.Add(PacketType.ClientSpatial, 41);
			packetSizeLookup.Add(PacketType.ClientActions, 0);
		}

		private void ReceivedGameSettingsRequest(Message message)
		{
			SendGameSettings(message);
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

			var clientSpatialData = new ClientSpatial();
			clientSpatialData.TimeStamp = server.Reader.ReadTimeStamp();
			clientSpatialData.ClientId = clientId;
			clientSpatialData.Position = messageHelper.ReadVector3(server.Reader);
			clientSpatialData.Velocity = messageHelper.ReadVector3(server.Reader);
			clientSpatialData.Angle = messageHelper.ReadVector3(server.Reader);

			clients[clientSpatialData.ClientId].SpatialData.Add(clientSpatialData);

			// Keep a maximum of 100 entries
			if (clients[clientSpatialData.ClientId].SpatialData.Count > 100)
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

			int actions = server.Reader.ReadByte() / 5;

			for (int i = 0; i < actions; i++)
			{
				var action = new ClientAction();
				action.TimeStamp = server.Reader.ReadTimeStamp();
				action.Type = (ClientActionType)server.Reader.ReadByte();

				clients[clientId].Actions.Add(action);

				// Keep a maximum of 100 entries
				if (clients[clientId].Actions.Count > 100)
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
	}
}