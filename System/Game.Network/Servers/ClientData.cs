using System.Collections.Generic;
using Game.Network.Common;

namespace Game.Network.Servers
{
	public class ClientData
	{
		public List<ClientSpatialData> SpatialData { get; set; }

		public ClientData()
		{
			SpatialData = new List<ClientSpatialData>();
		}
	}
}