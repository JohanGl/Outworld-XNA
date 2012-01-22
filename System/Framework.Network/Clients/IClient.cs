using System.Collections.Generic;
using Framework.Network.Clients.Configurations;
using Framework.Network.Messages;
using Framework.Network.Messages.MessageReaders;
using Framework.Network.Messages.MessageWriters;

namespace Framework.Network.Clients
{
	public interface IClient
	{
		List<Message> Messages { get; set; }
		IMessageReader Reader { get; set; }
		IMessageWriter Writer { get; set; }

		void Initialize(IClientConfiguration configuration);
		void Connect();
		void Disconnect(string message = null);
		void Update();
		void Send(MessageDeliveryMethod method);

		bool IsConnected { get; }
		float TimeStamp { get; }
	}
}