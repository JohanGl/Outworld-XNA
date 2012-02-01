namespace Game.Network.Common
{
	public struct EntityEvent
	{
		public ushort Id;
		public float TimeStamp;

		public EntityEventType Type;
	}
}