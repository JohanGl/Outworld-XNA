using System;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Framework.Gui
{
	public class ImageButton : UIElement
	{
		public Texture2D texture;
		public object Tag;
		public bool IsPressed { get; private set; }

		public event EventHandler Click;

		private ButtonStates buttonStates;
		private Timer timerPressedDuration;
		private Timer timerClickEventPauseDuration;

		public ImageButton()
		{
			Initialize();
		}

		public ImageButton(Texture2D texture, ButtonStates buttonStates)
		{
			this.texture = texture;
			this.buttonStates = buttonStates;
			this.buttonStates.Validate();

			Opacity = 1.0f;

			Initialize();
		}

		private void Initialize()
		{
			timerPressedDuration = new Timer(100);
			timerPressedDuration.Elapsed += timerPressedDuration_Elapsed;

			timerClickEventPauseDuration = new Timer(200);
			timerClickEventPauseDuration.Elapsed += timerClickEventPauseDuration_Elapsed;
		}

		public override void SetFocus(bool state)
		{
			IsFocused = state;
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			Width = buttonStates.Default.Width;
			Height = buttonStates.Default.Height;

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
				spriteBatch.Draw(texture, Position, buttonStates.Pressed, Color.White * Opacity);
			}
			else if (IsFocused)
			{
				spriteBatch.Draw(texture, Position, buttonStates.Focused, Color.White * Opacity);
			}
			else if (IsHighlighted)
			{
				spriteBatch.Draw(texture, Position, buttonStates.Highlighted, Color.White * Opacity);
			}
			else
			{
				spriteBatch.Draw(texture, Position, buttonStates.Default, Color.White * Opacity);
			}
		}

		public void Press()
		{
			IsPressed = true;
			timerPressedDuration.Start();
			timerClickEventPauseDuration.Start();
		}

		private void timerPressedDuration_Elapsed(object sender, ElapsedEventArgs e)
		{
			IsPressed = false;
			timerPressedDuration.Stop();
		}

		private void timerClickEventPauseDuration_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			timerClickEventPauseDuration.Stop();

			if (Click != null)
			{
				Click(this, EventArgs.Empty);
			}
		}
	}
}