using Framework.Core.Contexts;
using Microsoft.Xna.Framework;

namespace Game.Core.Utils
{
	public class FpsCounter
	{
		private int frameCount;
		private double timeSinceLastUpdate = 0.0d;
		private GameContext context;

		public FpsCounter(GameContext context)
		{
			this.context = context;
		}

		public void Update(GameTime gameTime)
		{
			frameCount++;

			double elapsed = gameTime.ElapsedGameTime.TotalSeconds;
			timeSinceLastUpdate += elapsed;
 
			if (timeSinceLastUpdate > 1.0d)
			{
				context.Graphics.Fps = (float)(frameCount / timeSinceLastUpdate);

				frameCount = 0;
				timeSinceLastUpdate -= 1.0d;
			}
		}
	}
}