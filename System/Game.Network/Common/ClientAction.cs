using System;

namespace Game.Network.Common
{
	public struct ClientAction
	{
		public DateTime Time;

		public byte ClientId;
		public ClientActionType Type;
	}
}