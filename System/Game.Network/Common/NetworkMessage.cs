namespace Game.Network.Common
{
	/// <summary>
	/// Used to send messages of server/client info
	/// </summary>
	public class NetworkMessage
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
		EntityEvent,
		Chat
	}
}