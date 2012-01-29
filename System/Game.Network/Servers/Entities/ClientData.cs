using System.Collections.Generic;
using Game.Network.Common;

namespace Game.Network.Servers
{
	public class ClientData
	{
		public int Timeout;
		public List<ClientSpatial> SpatialData { get; set; }
		public List<ClientAction> Actions { get; set; }
		public List<byte> ActionsSequencesToAcknowledgeOnNextUpdate { get; set; }

		public ClientData()
		{
			SpatialData = new List<ClientSpatial>(110);
			Actions = new List<ClientAction>(110);
			ActionsSequencesToAcknowledgeOnNextUpdate = new List<byte>(255);
			Timeout = int.MaxValue;
		}
	}
}