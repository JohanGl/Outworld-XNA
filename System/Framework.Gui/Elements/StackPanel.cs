using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class StackPanel : UIElement
	{
		public Orientation Orientation;
		public List<UIElement> Children;
		public Thickness Spacing;

		public StackPanel()
		{
			Orientation = Orientation.Vertical;
			Children = new List<UIElement>();
			Spacing = new Thickness(0);
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			guiManager.Arrange(this, availableSize);

			Vector2 currentPosition = Position;

			if (Orientation == Orientation.Vertical)
			{
				// Set the position of each child element relative to the stackpanel
				foreach (var child in Children)
				{
					currentPosition.Y += Spacing.Top;

					child.Position = currentPosition;

					var stackPanelAvailableSize = new Rectangle((int)currentPosition.X, (int)currentPosition.Y, (int)Width, (int)Height);

					child.UpdateLayout(guiManager, stackPanelAvailableSize);

					currentPosition.Y += child.Height;
					currentPosition.Y += Spacing.Bottom;
				}
			}
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			// Render each child element within the stackpanel
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

	public enum Orientation
	{
		Horizontal,
		Vertical
	}
}