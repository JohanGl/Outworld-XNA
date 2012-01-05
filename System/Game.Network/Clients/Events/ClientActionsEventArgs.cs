using System;
using System.Collections.Generic;
using Game.Network.Common;

namespace Game.Network.Clients.Events
{
	public class ClientActionsEventArgs : EventArgs
	{
		public List<ClientAction> ClientActions;

		public ClientActionsEventArgs()
		{
			ClientActions = new List<ClientAction>();
		}
	}
}