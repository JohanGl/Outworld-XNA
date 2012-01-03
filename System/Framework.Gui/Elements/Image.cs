using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Gui
{
	public class Image : UIElement
	{
		public Texture2D Source;

		public Image(Texture2D source = null)
		{
			Source = source;
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			if (Width == 0f)
			{
				Width = Source.Width;
			}

			if (Height == 0f)
			{
				Height = Source.Height;
			}

			guiManager.Arrange(this, availableSize);
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Source, new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height), Color.White);

			//spriteBatch.Draw(Source, Position, Color.White);
		}
	}
}