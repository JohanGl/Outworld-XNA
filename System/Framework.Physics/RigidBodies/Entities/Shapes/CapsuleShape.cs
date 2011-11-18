namespace Framework.Physics.RigidBodies.Shapes
{
	public class CapsuleShape : IRigidBodyShape
	{
		public float Length;
		public float Radius;

		public CapsuleShape(float length, float radius)
		{
			Length = length;
			Radius = radius;
		}
	}
}