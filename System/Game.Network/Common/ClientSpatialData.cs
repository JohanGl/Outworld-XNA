using System;
using Microsoft.Xna.Framework;

namespace Game.Network.Common
{
	public struct ClientSpatialData
	{
		public DateTime Time;

		public byte ClientId;
		public Vector3 Position;
		public Vector3 Velocity;
		public Vector3 Angle;
	}
}