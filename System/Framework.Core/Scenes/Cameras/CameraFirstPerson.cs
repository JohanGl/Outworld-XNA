using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Core.Scenes.Cameras
{
	public class CameraFirstPerson : CameraBase
	{
		public void Initialize(Viewport viewPort)
		{
			World = Matrix.Identity;
			Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, viewPort.AspectRatio, 0.1f, 800.0f);
		}

		public override void LookAt(Vector3 from, Vector3 to)
		{
			Position = from;
			Target = to;

			View = Matrix.CreateLookAt(Position, Target, Vector3.Up);
		}
	}
}