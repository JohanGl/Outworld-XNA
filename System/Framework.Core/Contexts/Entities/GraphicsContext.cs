using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Core.Contexts
{
	public class GraphicsContext
	{
		public GraphicsDeviceManager DeviceManager;
		public GraphicsDevice Device;
		public Effect Effect;
		public SpriteBatch SpriteBatch;
		public float Fps;
	}
}