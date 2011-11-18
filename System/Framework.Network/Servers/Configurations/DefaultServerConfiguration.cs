using System.Net;

namespace Framework.Network.Servers.Configurations
{
	public class DefaultServerConfiguration : IServerConfiguration
	{
		public string LocalAddress { get; set; }
		public int Port { get; set; }
		public int MaximumConnections { get; set; }
	}
}