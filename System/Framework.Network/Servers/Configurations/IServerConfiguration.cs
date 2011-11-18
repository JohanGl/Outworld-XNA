namespace Framework.Network.Servers.Configurations
{
	public interface IServerConfiguration
	{
		string LocalAddress { get; set; }
		int Port { get; set; }
		int MaximumConnections { get; set; }
	}
}