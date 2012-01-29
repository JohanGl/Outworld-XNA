namespace Game.Network.Common
{
	public struct ClientAction
	{
		public byte ClientId;
		public float TimeStamp;

		public ServerEntityEventType Type;
	}
}