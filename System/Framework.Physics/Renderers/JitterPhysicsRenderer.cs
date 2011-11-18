using Framework.Physics.Renderers.Entities;
using Jitter.Collision.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Jitter.LinearMath;

namespace Framework.Physics.Renderers
{
	public class JitterPhysicsRenderer : IPhysicsRenderer
	{
		private Jitter.World world;

		private BoxPrimitive box;
		private CapsulePrimitive capsule;
		private BasicEffect effect;

		public JitterPhysicsRenderer(GraphicsDevice device, BasicEffect effect, Jitter.World world)
		{
			this.world = world;
			this.effect = effect;

			box = new BoxPrimitive(device);
			capsule = new CapsulePrimitive(device);
		}

		public void Render()
		{
			//GeometricPrimitive primitive = box;
			//Matrix scale = Matrix.CreateScale(ToXNAVector(new JVector(0.3f, 1.9f, 0.3f)));

			//primitive.AddWorldMatrix(scale * Matrix.CreateTranslation(ToXNAVector(new JVector(16.35f, 15f + 1.9f, 16.35f))));
			//primitive.Draw(effect);

			foreach (var body in world.RigidBodies)
			{
				GeometricPrimitive primitive = null;
				Matrix scale = new Matrix();

				if (body.Shape is BoxShape)
				{
					primitive = box;
					scale = Matrix.CreateScale(ToXNAVector((body.Shape as BoxShape).Size));
				}
				else if (body.Shape is CapsuleShape)
				{
					primitive = capsule;
					scale = Matrix.CreateScale((body.Shape as CapsuleShape).Radius * 2, (body.Shape as CapsuleShape).Length, (body.Shape as CapsuleShape).Radius * 2);
				}

				primitive.AddWorldMatrix(scale * ToXNAMatrix(body.Orientation) * Matrix.CreateTranslation(ToXNAVector(body.Position)));
				primitive.Draw(effect);
			}
		}

		#region Jitter converters

		private JVector ToJitterVector(Vector3 vector)
		{
			return new JVector(vector.X, vector.Y, vector.Z);
		}

		private Matrix ToXNAMatrix(JMatrix matrix)
		{
			return new Matrix(matrix.M11,
							matrix.M12,
							matrix.M13,
							0.0f,
							matrix.M21,
							matrix.M22,
							matrix.M23,
							0.0f,
							matrix.M31,
							matrix.M32,
							matrix.M33,
							0.0f, 0.0f, 0.0f, 0.0f, 1.0f);
		}

		private JMatrix ToJitterMatrix(Matrix matrix)
		{
			JMatrix result;
			result.M11 = matrix.M11;
			result.M12 = matrix.M12;
			result.M13 = matrix.M13;
			result.M21 = matrix.M21;
			result.M22 = matrix.M22;
			result.M23 = matrix.M23;
			result.M31 = matrix.M31;
			result.M32 = matrix.M32;
			result.M33 = matrix.M33;
			return result;
		}

		private Vector3 ToXNAVector(JVector vector)
		{
			return new Vector3(vector.X, vector.Y, vector.Z);
		}

		#endregion
	}
}