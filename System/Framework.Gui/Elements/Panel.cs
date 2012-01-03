using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class Panel : UIElement
	{
		public List<UIElement> Children;
		public Thickness Spacing;

		public Panel()
		{
			Children = new List<UIElement>();
			Spacing = new Thickness(0);
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			guiManager.Arrange(this, availableSize);

			// Set the position of each child element relative to the panel
			foreach (var child in Children)
			{
				var panelAvailableSize = new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);

				child.UpdateLayout(guiManager, panelAvailableSize);
			}
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			// Render each child element within the panel
			foreach (var child in Children)
			{
				child.Render(device, spriteBatch);
			}
		}

		public override void HandleMouseEvent(MouseState mouseState)
		{
			foreach (UIElement childElement in Children)
			{
				if (mouseState.LeftButton == ButtonState.Pressed)
				{
					if (childElement.IsInside(mouseState.X, mouseState.Y))
					{
						childElement.HandleMouseEvent(mouseState);
					}
				}
			}
		}
	}
}