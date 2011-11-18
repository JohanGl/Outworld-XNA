namespace Game.Network.Common
{
	public enum GameClientMessageType : byte
	{
		Unknown = 0,
		Connect,
		Disconnect,
		GameSettings,
		ClientSpatial,
		ClientStatus
	}
}