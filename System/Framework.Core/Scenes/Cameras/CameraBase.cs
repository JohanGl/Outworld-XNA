using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Core.Scenes.Cameras
{
	/// <summary>
	/// Camera base class
	/// </summary>
	public abstract class CameraBase
	{
		public Vector3 Position;
		public Vector3 Target;

		/// <summary>
		/// Gets the directional normal of the cameras position and target
		/// </summary>
		public Vector3 LookAtNormal
		{
			get
			{
				Vector3 vector = (Target - Position);
				vector.Normalize();
				return vector;
			}
		}

		public Matrix World;
		public Matrix View;
		public Matrix Projection;

		public BoundingBox GetBoundingBox(Vector3 size)
		{
			Vector3 half = size / 2;
			return new BoundingBox(Position - half, Position + half);
		}

		public abstract void LookAt(Vector3 from, Vector3 to);

		/// <summary>
		/// Applies the camera viewing properties to an effect
		/// </summary>
		/// <param name="effect"></param>
		public void ApplyToEffect(BasicEffect effect)
		{
			effect.World = World;
			effect.View = View;
			effect.Projection = Projection;
		}
	}
}