using System.Collections.Generic;
using Game.Network.Common;

namespace Game.Network.Servers
{
	public class ClientData
	{
		public long SystemTime;
		public int Timeout;
		public List<ClientSpatialData> SpatialData { get; set; }
		public List<ClientAction> Actions { get; set; }

		public ClientData()
		{
			SpatialData = new List<ClientSpatialData>(10);
			Actions = new List<ClientAction>(10);
			Timeout = int.MaxValue;
		}
	}
}