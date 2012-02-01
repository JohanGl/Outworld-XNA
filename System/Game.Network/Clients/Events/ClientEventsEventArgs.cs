using System;
using System.Collections.Generic;
using Game.Network.Common;

namespace Game.Network.Clients.Events
{
	public class ClientEventsEventArgs : EventArgs
	{
		public List<EntityEvent> Events;

		public ClientEventsEventArgs()
		{
			Events = new List<EntityEvent>();
		}
	}
}