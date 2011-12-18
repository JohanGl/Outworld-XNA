using Framework.Core.Messaging;

namespace Game.Network.Clients
{
	/// <summary>
	/// Used to send messages of server/client type to IMessageHandler
	/// </summary>
	public class NetworkMessage : IMessage
	{
		public MessageType Type;
		public string Text;

		public enum MessageType
		{
			Connected,
			Disconnected,
			Chat
		}
	}
}