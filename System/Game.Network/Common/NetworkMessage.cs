using Framework.Core.Messaging;
using Game.Network.Common;

namespace Game.Network.Clients
{
	/// <summary>
	/// Used to send messages of server/client type to IMessageHandler
	/// </summary>
	public class NetworkMessage : IMessage
	{
		public MessageType Type;
		public ClientActionType ClientActionType;
		public string Text;

		public enum MessageType
		{
			Connected,
			Disconnected,
			ClientAction,
			Chat
		}
	}
}