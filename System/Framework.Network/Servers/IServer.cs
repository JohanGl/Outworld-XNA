using System.Collections.Generic;
using Framework.Network.Messages;
using Framework.Network.Messages.MessageReaders;
using Framework.Network.Messages.MessageWriters;
using Framework.Network.Servers.Configurations;

namespace Framework.Network.Servers
{
	public interface IServer
	{
		List<Message> Messages { get; set; }
		IMessageReader Reader { get; set; }
		IMessageWriter Writer { get; set; }

		void Initialize(IServerConfiguration configuration);
		void Start();
		void Stop(string message = null);
		void Update();
		void Send(long clientId, MessageDeliveryMethod method);
		void Broadcast(MessageDeliveryMethod method, long? excludedClientId = null);

		bool IsStarted { get; }
		float TimeStamp { get; }
	}
}