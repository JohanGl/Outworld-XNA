using System;
using Game.Network.Common;

namespace Game.Network.Clients.Events
{
	public class ClientSpatialEventArgs : EventArgs
	{
		public ClientSpatialData[] ClientData;
	}
}