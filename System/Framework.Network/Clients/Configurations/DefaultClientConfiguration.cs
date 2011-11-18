using System.Net;

namespace Framework.Network.Clients.Configurations
{
	public class DefaultClientConfiguration : IClientConfiguration
	{
		public string ServerAddress { get; set; }
		public int ServerPort { get; set; }
	}
}