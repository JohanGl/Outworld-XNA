using Framework.Network.Messages;

namespace NetworkTool
{
	public class Message
	{
		public long ClientId { get; set; }
		public byte[] Data { get; set; }
		public string DataRaw { get; set; }
		public float RemoteTimeOffset { get; set; }
		public MessageType Type { get; set; }
	}
}