using Microsoft.Xna.Framework;

namespace NetworkTool
{
	public struct EntitySpatial
	{
		public ushort Id;
		public float TimeStamp;

		public Vector3 Position;
		public Vector3 Velocity;
		public Vector3 Angle;
	}
}