using System;

namespace Framework.Network.Servers
{
	public class ClientStatusArgs : EventArgs
	{
		public byte ClientId;
		public ClientStatusType Type;
	}
}