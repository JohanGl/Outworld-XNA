namespace NetworkTool
{
	/// <summary>
	/// Defines all packet types (headers) set as the first byte of each packet for type identification
	/// </summary>
	public enum PacketType : byte
	{
		Unknown = 0,			// Default type to track incorrect code (code which forgets to set this type)
		Combined,				// Marks that the package is a combination of packages
		GameSettings,			// Startup information for when a client first joins a game
		EntityStatus,			// Connection and disconnection packet for remote entities
		EntitySpatial,			// Entity position, velocity and angles
		EntityEvents			// Events such as running, jumping, shooting etc
	}
}