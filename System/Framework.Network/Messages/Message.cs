namespace Framework.Network.Messages
{
	/// <summary>
	/// Defines the format of the packets being handled on the server and clients
	/// </summary>
	public struct Message
	{
		public MessageType Type;
		public long ClientId;
		public byte[] Data;
		public float RemoteTimeOffset;
	}

	public enum MessageType
	{
		Data,
		Connect,
		Disconnect
	}
}