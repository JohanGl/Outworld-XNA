namespace Framework.Network.Clients.Configurations
{
	public interface IClientConfiguration
	{
		string ServerAddress { get; set; }
		int ServerPort { get; set; }
	}
}