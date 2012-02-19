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
			packetSizeLookup.Add(PacketType.EntitySpatial, 41);
			packetSizeLookup.Add(PacketType.EntityEvents, 0);
		}

		private void ReceivedGameSettingsRequest(Message message)
		{
			SendGameSettings(message);
		}

		private void ReceivedEntitySpatial(Message message)
		{
			var clientId = connectionIds[message.ClientId];

			if (!entityHelper.Entities.ContainsKey(clientId))
			{
				return;
			}

			server.Reader.ReadNewMessage(message);
			server.Reader.ReadByte();

			var spatial = new EntitySpatial();
			spatial.Id = clientId;
			spatial.TimeStamp = server.Reader.ReadTimeStamp();
			spatial.Position = messageHelper.ReadVector3(server.Reader);
			spatial.Velocity = messageHelper.ReadVector3(server.Reader);
			spatial.Angle = messageHelper.ReadVector3(server.Reader);

			entityHelper.AddSpatial(clientId, spatial);
		}

		private void ReceivedEntityEvents(Message message)
		{
			var clientId = connectionIds[message.ClientId];

			if (!entityHelper.Entities.ContainsKey(clientId))
			{
				return;
			}

			// Header
			server.Reader.ReadNewMessage(message);
			server.Reader.ReadByte();

			// Events
			int events = server.Reader.ReadByte() / 5;

			for (int i = 0; i < events; i++)
			{
				var entityEvent = new EntityEvent();
				entityEvent.TimeStamp = server.Reader.ReadTimeStamp();
				entityEvent.Type = (EntityEventType)server.Reader.ReadByte();

				entityHelper.AddEvent(clientId, entityEvent);
			}
		}

		private void ReceivedCombinedMessage(Message message)
		{
			var clientId = connectionIds[message.ClientId];

			if (!entityHelper.Entities.ContainsKey(clientId))
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
				if (type == PacketType.EntityEvents)
				{
					// The byte at (combinedMessageStartIndex + 1) represents the length of all events, then we add + 2 for the header to get the total packet size
					size = message.Data[combinedMessageStartIndex + 1] + 2;
				}
				else
				{
					throw new NotImplementedException("Implement this type!");
				}
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