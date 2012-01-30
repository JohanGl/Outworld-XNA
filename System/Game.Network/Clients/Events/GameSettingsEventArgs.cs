using System;
using Microsoft.Xna.Framework;

namespace Game.Network.Clients
{
	public class GameSettingsEventArgs : EventArgs
	{
		public ushort ClientId;
		public int Seed;
		public Vector3 Gravity;
	}
}