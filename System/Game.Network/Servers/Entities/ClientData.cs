using System.Collections.Generic;
using Game.Network.Common;

namespace Game.Network.Servers
{
	public class ClientData
	{
		public int Timeout;
		public List<ClientSpatial> SpatialData { get; set; }
		public List<ClientAction> Actions { get; set; }

		public ClientData()
		{
			SpatialData = new List<ClientSpatial>(100 + 10);
			Actions = new List<ClientAction>(100 + 10);
			Timeout = int.MaxValue;
		}
	}
}