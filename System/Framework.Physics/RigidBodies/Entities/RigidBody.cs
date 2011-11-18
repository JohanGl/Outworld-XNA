using Framework.Physics.RigidBodies.Shapes;
using Microsoft.Xna.Framework;

namespace Framework.Physics.RigidBodies
{
	public class RigidBody
	{
		public string Id { get; private set; }
		public string Group { get; private set; }

		/// <summary>
		/// Gets or sets the handler associated with the rigid body
		/// </summary>
		public IRigidBodyHandler RigidBodyHandler;

		public IRigidBodyShape Shape;

		public Vector3 Position;
		public Matrix Orientation;
		public Vector3 Velocity;
		public float Mass;
		public bool IsStatic;
		public object Tag;

		public RigidBody(string id, string group = "Default")
		{
			Id = id;
			Group = group;

			Position = new Vector3();
			Orientation = new Matrix();
			Velocity = new Vector3();

			Mass = 1f;
		}
	}
}