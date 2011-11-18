using Microsoft.Xna.Framework;

namespace Framework.Physics.RigidBodies.Shapes
{
	public class BoxShape : IRigidBodyShape
	{
		public Vector3 Size;

		public BoxShape(Vector3 size)
		{
			Initialize(size.X, size.Y, size.Z);
		}

		public BoxShape(float size)
		{
			Initialize(size, size, size);
		}

		public BoxShape(float width, float height, float length)
		{
			Initialize(width, height, length);
		}

		private void Initialize(float width, float height, float length)
		{
			Size = new Vector3(width, height, length);
		}
	}
}