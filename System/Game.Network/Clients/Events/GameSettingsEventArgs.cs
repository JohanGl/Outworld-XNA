using System;

namespace Game.Network.Clients
{
	public class GameSettingsEventArgs : EventArgs
	{
		public ushort ClientId;
		public int Seed;
	}
}