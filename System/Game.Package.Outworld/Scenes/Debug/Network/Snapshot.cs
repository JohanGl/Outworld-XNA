using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.Debug.Network
{
	class Snapshot
	{
		public TimeSpan Time;
		public Vector2 Position;

		public Snapshot(TimeSpan time, Vector2 position)
		{
			Time = time;
			Position = position;
		}
	}
}
