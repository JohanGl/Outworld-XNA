using System;
using Microsoft.Xna.Framework;

namespace Game.Network.Clients
{
	public class GameSettingsEventArgs : EventArgs
	{
		public byte ClientId;
		public int Seed;
		public Vector3 Gravity;
	}
}