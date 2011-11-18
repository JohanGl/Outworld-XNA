using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Timers;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class Button : UIElement
	{
		public Texture2D Default;
		public Texture2D Selected;
		public Texture2D Pressed;
		public object Tag;
		public bool IsPressed { get; private set; }

		public event EventHandler Click;

		private TextBlock textBlock;
		private Timer timerPressedDuration;

		public Button()
		{
			Initialize();
		}

		public Button(string text, SpriteFont font, Texture2D defaultState, Texture2D selectedState, Texture2D pressedState)
		{
			textBlock = new TextBlock(text, font);
			Default = defaultState;
			Selected = selectedState;
			Pressed = pressedState;

			Opacity = 1.0f;

			Initialize();
		}

		private void Initialize()
		{
			timerPressedDuration = new Timer(100);
			timerPressedDuration.Elapsed += timer_Elapsed;
		}

		public override void SetFocus(bool state)
		{
			IsFocused = state;
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			Width = Default.Width;
			Height = Default.Height;

			guiManager.Arrange(this, availableSize);

			textBlock.HorizontalAlignment = this.HorizontalAlignment;
			textBlock.VerticalAlignment = this.VerticalAlignment;
			textBlock.Margin = this.Margin;
			textBlock.UpdateLayout(guiManager, availableSize);
		}

		public override void HandleMouseEvent(MouseState mouseState)
		{
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				if (IsInside(mouseState.X, mouseState.Y))
				{
					Press();
				}
			}
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			if (IsPressed)
			{
				spriteBatch.Draw(Pressed, Position, Color.White * Opacity);
			}
			else if (IsFocused)
			{
				spriteBatch.Draw(Selected, Position, Color.White * Opacity);
			}
			else
			{
				spriteBatch.Draw(Default, Position, Color.White * Opacity);
			}

			textBlock.Render(device, spriteBatch);
		}

		public void Press()
		{
			IsPressed = true;
			timerPressedDuration.Start();

			if (Click != null)
			{
				Click(this, EventArgs.Empty);
			}
		}

		private void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			IsPressed = false;
			timerPressedDuration.Stop();
		}
	}
}