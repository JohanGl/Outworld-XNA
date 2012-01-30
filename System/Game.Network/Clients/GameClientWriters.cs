using System.Collections.Generic;
using Framework.Network.Messages;
using Game.Network.Common;
using Microsoft.Xna.Framework;

namespace Game.Network.Clients
{
	public partial class GameClient
	{
		// Handled combined messages
		private bool isCombined;
		private bool isCombinedInitialized;
		
		//private List<RecordedMessage> recordedMessages = new List<RecordedMessage>();

		public void GetGameSettings()
		{
			client.Writer.WriteNewMessage();
			client.Writer.Write((byte)PacketType.GameSettings);
			client.Writer.Write(client.TimeStamp);
			client.Send(MessageDeliveryMethod.ReliableUnordered);
		}

		public void SendClientSpatial(Vector3 position, Vector3 velocity, Vector3 angle)
		{
			InitializeMessageWriter();
			client.Writer.Write((byte)PacketType.EntitySpatial);
			client.Writer.WriteTimeStamp();
			messageHelper.WriteVector3(position, client.Writer);
			messageHelper.WriteVector3(velocity, client.Writer);
			messageHelper.WriteVector3(angle, client.Writer);
			SendMessage();

			// Recorded messages
			//var recordedMessage = new RecordedMessage();
			//recordedMessage.Spatial = new ClientSpatial();
			//recordedMessage.Spatial.TimeStamp = client.TimeStamp;
			//recordedMessage.Spatial.Position = position;
			//recordedMessage.Spatial.Velocity = velocity;
			//recordedMessage.Spatial.Angle = angle;
			//recordedMessages.Add(recordedMessage);

			//if (recordedMessages.Count == 1200)
			//{
			//    var dumper = new ObjectDumper();
			//    File.WriteAllText("d:\\recording.xml", dumper.ObjectToXml(recordedMessages));
			//    recordedMessages.Clear();
			//}
		}

		public void SendClientActions(List<EntityEvent> actions)
		{
			InitializeMessageWriter();
			client.Writer.Write((byte)PacketType.EntityEvents);
			client.Writer.Write((byte)(actions.Count * 5));

			for (int i = 0; i < actions.Count; i++)
			{
				client.Writer.Write(actions[i].TimeStamp);
				client.Writer.Write((byte)actions[i].Type);
			}

			SendMessage(MessageDeliveryMethod.ReliableUnordered);

			// Recorded messages
			//var recordedMessage = new RecordedMessage();
			//recordedMessage.Actions = new List<ClientAction>();
			//recordedMessage.Actions.AddRange(actions);
			//recordedMessages.Add(recordedMessage);
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

		private void SendMessage(MessageDeliveryMethod method = MessageDeliveryMethod.Unreliable)
		{
			if (!isCombined)
			{
				client.Send(method);
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