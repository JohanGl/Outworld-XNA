using Framework.Core.Messaging;

namespace Game.Network.Common
{
	/// <summary>
	/// Used to send messages of server/client type to IMessageHandler
	/// </summary>
	public class NetworkMessage : IMessage
	{
		public ushort ClientId;
		public NetworkMessageType Type;
		public EntityEventType EntityEventType;
		public string Text;
	}

	public enum NetworkMessageType
	{
		Connected,
		Disconnected,
		ClientAction,
		Chat
	}
}