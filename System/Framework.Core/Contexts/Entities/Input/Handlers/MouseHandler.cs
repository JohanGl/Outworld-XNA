using System;
using System.Collections.Generic;
using Framework.Core.Common;
using Microsoft.Xna.Framework.Input;

namespace Framework.Core.Contexts
{
	public class MouseHandler
	{
		public bool IsEnabled { get; private set; }
		public Vector2i Position { get; private set; }
		public Dictionary<MouseInputType, InputState> MouseState;

		public bool ShowCursor
		{
			get
			{
				return context.Game.IsMouseVisible;
			}

			set
			{
				context.Game.IsMouseVisible = value;
			}
		}

		/// <summary>
		/// Gets or sets the flag indicating whether the mouse position should be locked to the center of the viewport or not
		/// </summary>
		public bool AutoCenter
		{
			get
			{
				return autoCenter;
			}

			set
			{
				autoCenter = value;

				if (autoCenter && context.Game.IsActive)
				{
					// Set the mouse at the center of the game window
					Mouse.SetPosition(context.View.Area.Center.X, context.View.Area.Center.Y);
					previousState = Mouse.GetState();
				}
			}
		}

		private bool autoCenter;
		private GameContext context;
		private MouseState previousState;

		public MouseHandler(GameContext context)
		{
			this.context = context;

			IsEnabled = Environment.OSVersion.Platform != PlatformID.Xbox;
			MouseState = new Dictionary<MouseInputType, InputState>();
			previousState = Mouse.GetState();
			Position = new Vector2i(previousState.X, previousState.Y);

			// Map all button types of the gamepad per default
			foreach (int i in Enum.GetValues(typeof(MouseInputType)))
			{
				MouseState.Add((MouseInputType)i, new InputState());
			}
		}

		public void Update()
		{
			if (!IsEnabled)
			{
				return;
			}

			var currentState = Mouse.GetState();

			Position = new Vector2i(currentState.X, currentState.Y);

			// Loop through all mapped keys
			foreach (var pair in MouseState)
			{
				pair.Value.Clear();
				MouseInputType type = pair.Key;

				switch (type)
				{
					case MouseInputType.LeftButton:
						SetMouseButtonState(type, currentState.LeftButton, previousState.LeftButton);
						break;

					case MouseInputType.MiddleButton:
						SetMouseButtonState(type, currentState.MiddleButton, previousState.MiddleButton);
						break;

					case MouseInputType.RightButton:
						SetMouseButtonState(type, currentState.RightButton, previousState.RightButton);
						break;

					case MouseInputType.ScrollWheelUp:
						SetMouseAnalogState(type, currentState.ScrollWheelValue, previousState.ScrollWheelValue);
						break;

					case MouseInputType.ScrollWheelDown:
						SetMouseAnalogState(type, currentState.ScrollWheelValue, previousState.ScrollWheelValue);
						break;

					case MouseInputType.MoveUp:
						SetMouseAnalogState(type, currentState.Y, previousState.Y);
						break;

					case MouseInputType.MoveDown:
						SetMouseAnalogState(type, currentState.Y, previousState.Y);
						break;

					case MouseInputType.MoveLeft:
						SetMouseAnalogState(type, currentState.X, previousState.X);
						break;

					case MouseInputType.MoveRight:
						SetMouseAnalogState(type, currentState.X, previousState.X);
						break;
				}
			}

			// Auto center the mouse position?
			if (AutoCenter && context.Game.IsActive)
			{
				// Lock the mouse at the center of the game window (we need to call Mouse.GetState() again in order for this to work as expected)
				Mouse.SetPosition(context.View.Area.Center.X, context.View.Area.Center.Y);
				currentState = Mouse.GetState();
			}

			// Store the keyboard state
			previousState = currentState;
		}

		private void SetMouseButtonState(MouseInputType button, ButtonState state, ButtonState previousState)
		{
			// The button is pressed
			if (state == ButtonState.Pressed)
			{
				MouseState[button].Pressed = true;
				MouseState[button].Value = 1;

				// If the button was just pressed
				if (previousState != ButtonState.Pressed)
				{
					MouseState[button].WasJustPressed = true;
				}
			}
				// The button is released
			else
			{
				// If the button was just released
				if (previousState == ButtonState.Pressed)
				{
					MouseState[button].WasJustReleased = true;
				}
			}
		}

		private void SetMouseAnalogState(MouseInputType button, int current, int previous)
		{
			float delta = previous - current;

			// The button is pressed
			if (delta != 0)
			{
				MouseState[button].Pressed = true;
				MouseState[button].Value = delta * 0.03f;
			}
		}
	}
}