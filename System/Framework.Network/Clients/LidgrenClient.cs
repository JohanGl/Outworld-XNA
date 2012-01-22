using System.Collections.Generic;
using Framework.Core.Diagnostics.Logging;
using Framework.Network.Clients.Configurations;
using Framework.Network.Messages;
using Framework.Network.Messages.MessageReaders;
using Framework.Network.Messages.MessageWriters;
using Lidgren.Network;

namespace Framework.Network.Clients
{
	public class LidgrenClient : IClient
	{
		public List<Message> Messages { get; set; }
		public IMessageReader Reader { get; set; }
		public IMessageWriter Writer { get; set; }

		private NetClient client;
		private IClientConfiguration configuration;
		private NetIncomingMessage messageIn;

		public LidgrenClient()
		{
			Messages = new List<Message>();
		}

		public bool IsConnected
		{
			get
			{
				return (client.ConnectionStatus == NetConnectionStatus.Connected);
			}
		}

		public float TimeStamp
		{
			get
			{
				return (float)NetTime.Now;
			}
		}

		public void Initialize(IClientConfiguration configuration)
		{
			this.configuration = configuration;

			var netPeerConfiguration = new NetPeerConfiguration("LidgrenConfig");
			client = new NetClient(netPeerConfiguration);

			Reader = new LidgrenMessageReader(client);
			Writer = new LidgrenMessageWriter(client);
		}

		public void Connect()
		{
			client.Start();

			Writer.WriteNewMessage();
			Writer.Write("This is the hail message");
			client.Connect(configuration.ServerAddress, configuration.ServerPort, (NetOutgoingMessage)Writer.GetMessage());

			Logger.Log<LidgrenClient>(LogLevel.Debug, "Connect");
		}

		public void Disconnect(string message = null)
		{
			client.Shutdown(message);
			Logger.Log<LidgrenClient>(LogLevel.Debug, "Disconnect");
		}

		public void Update()
		{
			while ((messageIn = client.ReadMessage()) != null)
			{
				// handle incoming message
				switch (messageIn.MessageType)
				{
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.ErrorMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.VerboseDebugMessage:
						HandleLogMessage();
						break;

					case NetIncomingMessageType.StatusChanged:
						HandleStatusChangedMessage();
						break;

					case NetIncomingMessageType.Data:
						HandleDataMessage();
						break;

					default:
						HandleUnknownMessage();
						break;
				}
			}
		}

		public void Send(MessageDeliveryMethod method)
		{
			client.SendMessage((NetOutgoingMessage)Writer.GetMessage(), GetDeliveryMethod(method));
		}

		private NetDeliveryMethod GetDeliveryMethod(MessageDeliveryMethod method)
		{
			switch (method)
			{
				case MessageDeliveryMethod.Unreliable:
					return NetDeliveryMethod.Unreliable;

				case MessageDeliveryMethod.UnreliableSequenced:
					return NetDeliveryMethod.UnreliableSequenced;

				case MessageDeliveryMethod.ReliableSequenced:
					return NetDeliveryMethod.ReliableSequenced;

				case MessageDeliveryMethod.ReliableUnordered:
					return NetDeliveryMethod.ReliableUnordered;

				case MessageDeliveryMethod.ReliableOrdered:
					return NetDeliveryMethod.ReliableOrdered;

				default:
					return NetDeliveryMethod.Unknown;
			}
		}

		private void HandleLogMessage()
		{
			Logger.Log<LidgrenClient>(LogLevel.Debug, "Log: {0}", messageIn.ReadString());
		}

		private void HandleStatusChangedMessage()
		{
			var status = (NetConnectionStatus)messageIn.ReadByte();
			string reason = messageIn.ReadString();
			string text = NetUtility.ToHexString(messageIn.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason;

			Logger.Log<LidgrenClient>(LogLevel.Debug, "Status: {0}", text);
		}

		private void HandleDataMessage()
		{
			var message = new Message();
			message.Data = new byte[messageIn.LengthBytes];
			messageIn.ReadBytes(message.Data, 0, messageIn.LengthBytes);
			Messages.Add(message);

			//Logger.Log<LidgrenClient>(LogLevel.Debug, "Received Data: {0} bytes", message.Data.Length);
		}

		private void HandleUnknownMessage()
		{
			Logger.Log<LidgrenClient>(LogLevel.Debug, "Unknown: {0}", messageIn.ReadString());
		}
	}
}