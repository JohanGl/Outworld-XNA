namespace Game.Network.Servers.Settings
{
	public class GameServerSettings
	{
		public int MaximumConnections { get; set; }
		public int Port { get; set; }

		public WorldSettings World { get; set; }

		public GameServerSettings()
		{
			World = new WorldSettings();
		}
	}
}