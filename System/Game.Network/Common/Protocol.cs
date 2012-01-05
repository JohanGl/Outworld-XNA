namespace Game.Network.Common
{
	/// <summary>
	/// Defines all packet types (headers) set as the first byte of each packet for type identification
	/// </summary>
	public enum PacketType : byte
	{
		Unknown = 0,
		Combined,
		GameSettings,
		ClientStatus,
		ClientSpatial,
		ClientActions,
	}

	/// <summary>
	/// Defines all available entity actions/triggers/events within a packet
	/// </summary>
	public enum ClientActionType : byte
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