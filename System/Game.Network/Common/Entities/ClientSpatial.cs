using Microsoft.Xna.Framework;

namespace Game.Network.Common
{
	public struct ClientSpatial
	{
		public byte ClientId;
		public float TimeStamp;

		public Vector3 Position;
		public Vector3 Velocity;
		public Vector3 Angle;
	}
}