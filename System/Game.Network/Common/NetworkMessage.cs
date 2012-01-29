using Framework.Core.Messaging;
using Game.Network.Common;

namespace Game.Network.Clients
{
	/// <summary>
	/// Used to send messages of server/client type to IMessageHandler
	/// </summary>
	public class NetworkMessage : IMessage
	{
		public ushort ClientId;
		public NetworkMessageType Type;
		public ServerEntityEventType ServerEntityEventType;
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