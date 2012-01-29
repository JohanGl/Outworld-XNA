namespace Game.Network.Common
{
	/// <summary>
	/// Defines all packet types (headers) set as the first byte of each packet for type identification
	/// </summary>
	public enum PacketType : byte
	{
		Unknown = 0,			// Default type to track incorrect code (code which forgets to set this type)
		Combined,				// Marks that the package is a combination of packages
		GameSettings,			// Startup information for when a client first joins a game
		Sequence,				// Server packet sequence information used to track the sync of states between the server and client
		ClientStatus,			// Connection and disconnection packet for clients
		ClientSpatial,			// Client position, velocity and angles
		ClientActions			// Client actions such as running, jumping, shooting etc
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