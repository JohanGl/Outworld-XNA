using System;
using System.Collections.Generic;
using Framework.Core.Diagnostics.Logging;
using Framework.Network.Messages;
using Framework.Network.Messages.MessageReaders;
using Framework.Network.Messages.MessageWriters;
using Framework.Network.Servers.Configurations;
using Lidgren.Network;

namespace Framework.Network.Servers
{
	public class LidgrenServer : IServer
	{
		public event EventHandler<ClientStatusArgs> ClientStatusChanged;
		public List<Message> Messages { get; set; }
		public IMessageReader Reader { get; set; }
		public IMessageWriter Writer { get; set; }

		private NetServer server;
		private NetIncomingMessage messageIn;
		private Dictionary<long, NetConnection> connections;

		public LidgrenServer()
		{
			Messages = new List<Message>();
			connections = new Dictionary<long, NetConnection>();
		}

		public bool IsStarted
		{
			get
			{
				return server.Status == NetPeerStatus.Running;
			}
		}

		public void Initialize(IServerConfiguration configuration)
		{
			var netPeerConfiguration = new NetPeerConfiguration("LidgrenConfig");
			netPeerConfiguration.Port = configuration.Port;

			server = new NetServer(netPeerConfiguration);

			Reader = new DefaultMessageReader();
			Writer = new LidgrenMessageWriter(server);

			connections.Clear();
		}

		public void Start()
		{
			connections.Clear();
			server.Start();

			Logger.Log<LidgrenServer>(LogLevel.Debug, "Server started");
		}

		public void Stop(string message = null)
		{
			server.Shutdown(message);
			connections.Clear();
			Logger.Log<LidgrenServer>(LogLevel.Debug, "Stop");
		}

		public void Update()
		{
			while ((messageIn = server.ReadMessage()) != null)
			{
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

		public void Send(long clientId, MessageDeliveryMethod method)
		{
			if (connections.ContainsKey(clientId))
			{
				server.SendMessage((NetOutgoingMessage)Writer.GetMessage(), connections[clientId], GetDeliveryMethod(method));
			}
		}

		public void Broadcast(MessageDeliveryMethod method, long? excludedClientId)
		{
			NetConnection excluded = null;

			if (excludedClientId.HasValue &&
				connections.ContainsKey(excludedClientId.Value))
			{
				excluded = connections[excludedClientId.Value];
			}

			server.SendToAll((NetOutgoingMessage)Writer.GetMessage(), excluded, GetDeliveryMethod(method), 0);
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
			Logger.Log<LidgrenServer>(LogLevel.Debug, "Log: {0}", messageIn.ReadString());
		}

		private void HandleStatusChangedMessage()
		{
			var status = (NetConnectionStatus)messageIn.ReadByte();
			string reason = messageIn.ReadString();
			string text = NetUtility.ToHexString(messageIn.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason;

			UpdateConnectionsDictionary(status);

			Logger.Log<LidgrenServer>(LogLevel.Debug, "Status: {0}", text);
		}

		private void HandleDataMessage()
		{
			var message = new Message();
			message.ClientId = messageIn.SenderConnection.RemoteUniqueIdentifier;
			message.Data = new byte[messageIn.LengthBytes];
			messageIn.ReadBytes(message.Data, 0, messageIn.LengthBytes);
			Messages.Add(message);

			//Logger.Log<LidgrenServer>(LogLevel.Debug, "Received Data: {0} bytes", message.Data.Length);
		}

		private void HandleUnknownMessage()
		{
			Logger.Log<LidgrenServer>(LogLevel.Debug, "Unknown: {0}", messageIn.ReadString());
		}

		private void UpdateConnectionsDictionary(NetConnectionStatus status)
		{
			if (status == NetConnectionStatus.Connected)
			{
				var senderId = messageIn.SenderConnection.RemoteUniqueIdentifier;

				if (connections.ContainsKey(senderId))
				{
					connections.Remove(senderId);
				}

				connections.Add(senderId, messageIn.SenderConnection);

				// Store the event as a message
				var message = new Message
				{
					ClientId = messageIn.SenderConnection.RemoteUniqueIdentifier,
					Type = MessageType.Connect
				};

				Messages.Add(message);

				Logger.Log<LidgrenServer>(LogLevel.Info, string.Format("Client {0} connected from IP {1}", senderId, messageIn.SenderEndpoint.Address));
			}
			else if (status == NetConnectionStatus.Disconnected)
			{
				var senderId = messageIn.SenderConnection.RemoteUniqueIdentifier;

				if (connections.ContainsKey(senderId))
				{
					connections.Remove(senderId);
				}

				// Store the event as a message
				var message = new Message
				{
					ClientId = messageIn.SenderConnection.RemoteUniqueIdentifier,
					Type = MessageType.Disconnect
				};
	
				Messages.Add(message);

				Logger.Log<LidgrenServer>(LogLevel.Info, string.Format("Client {0} disconnected with IP {1}", senderId, messageIn.SenderEndpoint.Address));
			}
		}
	}
}