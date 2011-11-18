using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class TextBlock : UIElement
	{
		public string Text;
		public SpriteFont Font;

		public TextBlock(string text, SpriteFont font = null)
		{
			Text = text;
			Font = font;

			Opacity = 1.0f;
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			var bounds = Font.MeasureString(Text);

			Width = bounds.X;
			Height = bounds.Y;

			guiManager.Arrange(this, availableSize);
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.DrawString(Font, Text, Position, Color.White * Opacity);
		}

		public void SetSize(Vector2 size)
		{
			Height = size.Y;
			Width = size.X;
		}
	}
}