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
			Width = Source.Width;
			Height = Source.Height;

			guiManager.Arrange(this, availableSize);
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Source, Position, Color.White);
		}
	}
}