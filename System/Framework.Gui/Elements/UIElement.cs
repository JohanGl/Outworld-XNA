using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class UIElement
	{
		public string Id;
		public Vector2 Position;
		public float Width;
		public float Height;
		public HorizontalAlignment HorizontalAlignment;
		public VerticalAlignment VerticalAlignment;
		public Thickness Margin;
		//public Thickness Padding;
		public Visibility Visibility;
		public float Opacity;
		public bool IsFocused;
		public bool IsHighlighted;

		public virtual void SetFocus(bool state) { }
		public virtual void HandleMouseEvent(MouseState mouseState) { }
		public virtual void UpdateLayout(GuiManager guiManager, Rectangle availableSize) { }
		public virtual void Render(GraphicsDevice device, SpriteBatch spriteBatch) { }

		public bool IsInside(int x, int y)
		{
			if ((x >= Position.X && (x <= Position.X + Width)) &&
				(y >= Position.Y && (y <= Position.Y + Height)))
			{
				return true;
			}

			return false;
		}
	}

	public enum HorizontalAlignment
	{
		Stretch,
		Left,
		Center,
		Right
	}

	public enum VerticalAlignment
	{
		Stretch,
		Top,
		Center,
		Bottom
	}

	public struct Thickness
	{
		public float Left;
		public float Right;
		public float Top;
		public float Bottom;

		public Thickness(float uniformLength)
		{
			Left = uniformLength;
			Top = uniformLength;
			Right = uniformLength;
			Bottom = uniformLength;
		}

		public Thickness(float left, float top, float right, float bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}
	}

	public enum Visibility
	{
		Visible,
		Collapsed,
		Hidden
	}
}