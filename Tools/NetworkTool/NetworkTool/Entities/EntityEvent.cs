namespace NetworkTool
{
	public struct EntityEvent
	{
		public ushort Id;
		public float TimeStamp;

		public EntityEventType Type;
	}

	/// <summary>
	/// Defines all available entity events
	/// </summary>
	public enum EntityEventType : byte
	{
		Unknown = 0,
		Idle,
		RunDirection1,
		RunDirection2,
		RunDirection3,
		RunDirection4,
		RunDirection5,
		RunDirection6,
		RunDirection7,
		RunDirection8,
		Jump,
		Fall,
		Land,
		Crouch,
		Shoot,
		Reload,
		Damaged,
		Dead
	}
}