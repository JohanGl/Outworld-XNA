using System.Collections.Generic;
using Framework.Network.Clients;
using Framework.Network.Clients.Configurations;
using Framework.Network.Messages;
using Game.Network.Common;
using Microsoft.Xna.Framework;

namespace NetworkTool
{
	public class GameClient
	{
		private IClient client;

		public bool IsConnected { get { return client.IsConnected; } }

		public GameClient()
		{
			client = new LidgrenClient();

			var configuration = new DefaultClientConfiguration
			{
				ServerAddress = "127.0.0.1",
				ServerPort = 14242
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

		public void SendMessage(Message message)
		{
			client.Writer.WriteNewMessage();

			foreach (byte b in message.Data)
			{
				client.Writer.Write(b);
			}

			client.Send(MessageDeliveryMethod.ReliableOrdered);
		}

		//private MessageHelper messageHelper = new MessageHelper();
		//private bool isCombined;
		//private bool isCombinedInitialized;

		//public void SendSpatial(Vector3 position, Vector3 velocity, Vector3 angle)
		//{
		//    InitializeMessageWriter();
		//    client.Writer.Write((byte)PacketType.EntitySpatial);
		//    client.Writer.WriteTimeStamp();
		//    messageHelper.WriteVector3(position, client.Writer);
		//    messageHelper.WriteVector3(velocity, client.Writer);
		//    messageHelper.WriteVector3(angle, client.Writer);
		//    SendMessage();
		//}

		//public void SendEvents(List<EntityEvent> events)
		//{
		//    InitializeMessageWriter();
		//    client.Writer.Write((byte)PacketType.EntityEvents);
		//    client.Writer.Write((byte)(events.Count * 5));

		//    for (int i = 0; i < events.Count; i++)
		//    {
		//        client.Writer.Write(events[i].TimeStamp);
		//        client.Writer.Write((byte)events[i].Type);
		//    }

		//    SendMessage(MessageDeliveryMethod.ReliableUnordered);
		//}

		//private void InitializeMessageWriter()
		//{
		//    if (!isCombined)
		//    {
		//        client.Writer.WriteNewMessage();
		//    }
		//    else if (isCombined && !isCombinedInitialized)
		//    {
		//        client.Writer.WriteNewMessage();
		//        client.Writer.Write((byte)PacketType.Combined);
		//        isCombinedInitialized = true;
		//    }
		//}

		//private void SendMessage(MessageDeliveryMethod method = MessageDeliveryMethod.Unreliable)
		//{
		//    if (!isCombined)
		//    {
		//        client.Send(method);
		//    }
		//}

		//public void BeginCombinedMessage()
		//{
		//    isCombined = true;
		//    isCombinedInitialized = false;
		//}

		//public void EndCombinedMessage()
		//{
		//    client.Send(MessageDeliveryMethod.Unreliable);
		//    isCombined = false;
		//}
	}
}