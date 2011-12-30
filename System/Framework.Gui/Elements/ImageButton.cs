using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Timers;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class ImageButton : UIElement
	{
		public Texture2D texture;
		public object Tag;
		public bool IsPressed { get; private set; }

		public event EventHandler Click;

		private Rectangle defaultState;
		private Rectangle hoverState;
		private Rectangle focusedState;
		private Timer timerPressedDuration;

		public ImageButton()
		{
			Initialize();
		}

		public ImageButton(Texture2D texture, Rectangle defaultState, Rectangle hoverState, Rectangle focusedState)
		{
			this.texture = texture;
			this.defaultState = defaultState;
			this.hoverState = hoverState;
			this.focusedState = focusedState;

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
			Width = defaultState.Width;
			Height = defaultState.Height;

			guiManager.Arrange(this, availableSize);
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
				spriteBatch.Draw(texture, Position, focusedState, Color.White * Opacity);
			}
			else if (IsFocused)
			{
				spriteBatch.Draw(texture, Position, focusedState, Color.White * Opacity);
			}
			else
			{
				spriteBatch.Draw(texture, Position, defaultState, Color.White * Opacity);
			}
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