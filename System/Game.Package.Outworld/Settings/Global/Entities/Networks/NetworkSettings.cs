namespace Outworld.Settings.Global
{
	public class NetworkSettings
	{
		public string ServerAddress { get; set; }
		public int ServerPort { get; set; }
		public int ClientPort { get; set; }
		public int MaximumConnections { get; set; }
	}
}