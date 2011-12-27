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
	public enum PacketActionType : byte
	{
		Unknown = 0,
		Idle,
		Run,
		Jump,
		Crouch,
		Shoot,
		Reload,
		Damaged,
		Dead
	}
}