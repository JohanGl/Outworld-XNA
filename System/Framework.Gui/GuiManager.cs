using System;
using System.Collections.Generic;
using Framework.Core.Contexts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class GuiManager
	{
		private InputContext inputContext;
		private GraphicsDevice device;
		private SpriteBatch spriteBatch;
		private Rectangle availableSize;
		private UIElement currentlyHighlightedElement;
		private UIElement currentlyFocusedElement;

		public List<UIElement> Elements;
		public event EventHandler<ElementStateChangeArgs> ElementStateChanged;

		public GuiManager(InputContext inputContext, GraphicsDevice device, SpriteBatch spriteBatch = null)
		{
			this.inputContext = inputContext;
			this.device = device;
			this.spriteBatch = spriteBatch ?? new SpriteBatch(device);
			availableSize = device.Viewport.Bounds;

			Elements = new List<UIElement>();

			inputContext.Keyboard.KeyPressed += inputContext_KeyPressed;
		}

		private void inputContext_KeyPressed(object sender, KeyboardInputEventArgs e)
		{
			if (currentlyFocusedElement == null)
			{
				return;
			}

			if (currentlyFocusedElement.IsFocused)
			{
				if (currentlyFocusedElement is TextBox)
				{
					(currentlyFocusedElement as TextBox).UpdateText(e);
				}
				else if (currentlyFocusedElement is ChatBox)
				{
					(currentlyFocusedElement as ChatBox).UpdateText(e);
				}
			}
		}

		public void UpdateLayout()
		{
			for (int i = 0; i < Elements.Count; i++)
			{
				Elements[i].UpdateLayout(this, availableSize);
			}
		}

		public void UpdateInput()
		{
			UpdateMouseInput();
			//UpdateKeyboardInput();
		}

		private void UpdateMouseInput()
		{
			MouseState mouseState = Mouse.GetState();

			HandleLeftMouseButtonPressed(mouseState);
			HandleMouseWheel(mouseState);
			HandleMouseHover();
		}

		private UIElement GetElementAffectedByMouseEvent(List<UIElement> elements, bool boundsCheck = true)
		{
			for (int i = elements.Count - 1; i >= 0; i--)
			{
				// Current element to handle
				UIElement currentElement = elements[i];

				if (currentElement.Visibility == Visibility.Visible)
				{
					if (boundsCheck)
					{
						if (currentElement.IsInside(inputContext.Mouse.Position.X, inputContext.Mouse.Position.Y))
						{
							return GetChildElementAffectedByMouseEvent(currentElement);
						}
					}
					else
					{
						return GetChildElementAffectedByMouseEvent(currentElement);
					}
				}
			}

			return null;
		}

		private UIElement GetChildElementAffectedByMouseEvent(UIElement element)
		{
			if (element is Panel)
			{
				return GetElementAffectedByMouseEvent((element as Panel).Children);
			}
			else if (element is StackPanel)
			{
				return GetElementAffectedByMouseEvent((element as StackPanel).Children);
			}

			return element;
		}

		private void HandleLeftMouseButtonPressed(MouseState mouseState)
		{
			if (inputContext.Mouse.MouseState[MouseInputType.LeftButton].WasJustPressed)
			{
				var element = GetElementAffectedByMouseEvent(Elements);

				if (element != null)
				{
					if (currentlyFocusedElement != element)
					{
						element.IsFocused = true;

						if (currentlyFocusedElement != null)
						{
							currentlyFocusedElement.IsFocused = false;
						}

						currentlyFocusedElement = element;
					}

					element.HandleMouseEvent(mouseState);

					// Fire the state-change event
					if (ElementStateChanged != null)
					{
						ElementStateChanged(element, new ElementStateChangeArgs() { State = ElementState.Focused });
					}
				}
			}
		}

		private void HandleMouseWheel(MouseState mouseState)
		{
			if (mouseState.ScrollWheelValue != 0)
			{
				var element = GetElementAffectedByMouseEvent(Elements, false);

				if (element != null)
				{
					element.HandleMouseEvent(mouseState);
				}
			}
		}

		private void HandleMouseHover()
		{
			var element = GetElementAffectedByMouseEvent(Elements);

			if (element != null)
			{
				if (currentlyHighlightedElement != element)
				{
					element.IsHighlighted = true;

					if (currentlyHighlightedElement != null)
					{
						currentlyHighlightedElement.IsHighlighted = false;
					}

					currentlyHighlightedElement = element;

					// Fire the state-change event
					if (ElementStateChanged != null)
					{
						ElementStateChanged(element, new ElementStateChangeArgs() { State = ElementState.Highlighted });
					}
				}
			}
		}

		public void Render()
		{
			spriteBatch.Begin();

			for (int i = 0; i < Elements.Count; i++)
			{
				var element = Elements[i];

				if (element.Visibility == Visibility.Visible)
				{
					element.Render(device, spriteBatch);
				}
			}

			spriteBatch.End();
		}

		public void Arrange(UIElement element)
		{
			Arrange(element, availableSize);
		}

		public void Arrange(UIElement element, Rectangle availableSize)
		{
			var bounds = new Rectangle((int)element.Position.X, (int)element.Position.Y, (int)element.Width, (int)element.Height);

			// Set the X position
			switch (element.HorizontalAlignment)
			{
				case HorizontalAlignment.Left:
					element.Position.X = availableSize.Left;
					break;

				case HorizontalAlignment.Stretch:
				case HorizontalAlignment.Center:
					element.Position.X = availableSize.Center.X - (bounds.Width / 2);
					break;

				case HorizontalAlignment.Right:
					element.Position.X = availableSize.Right - bounds.Width;
					break;
			}

			// Set the Y position
			switch (element.VerticalAlignment)
			{
				case VerticalAlignment.Top:
					element.Position.Y = availableSize.Top;
					break;

				case VerticalAlignment.Stretch:
				case VerticalAlignment.Center:
					element.Position.Y = availableSize.Center.Y - (bounds.Height / 2);
					break;

				case VerticalAlignment.Bottom:
					element.Position.Y = availableSize.Bottom - bounds.Height;
					break;
			}

			// Apply margins
			element.Position.X += element.Margin.Left;
			element.Position.X -= element.Margin.Right;
			element.Position.Y += element.Margin.Top;
			element.Position.Y -= element.Margin.Bottom;
		}

		public void SetFocus(UIElement element)
		{
			ClearFocus();

			element.SetFocus(true);

			currentlyFocusedElement = element;
			currentlyHighlightedElement = null;
		}

		public void ClearFocus()
		{
			if (currentlyFocusedElement != null)
			{
				currentlyFocusedElement.SetFocus(false);
			}

			currentlyFocusedElement = null;
		}
	}
}