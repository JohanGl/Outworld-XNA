using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

// Button types: http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.buttons.aspx

namespace Framework.Core.Contexts.Input
{
	public class GamePadHandler
	{
		public bool IsEnabled { get; private set; }
		public Dictionary<Buttons, InputState> GamePadState;
		public event EventHandler<KeyboardInputEventArgs> KeyPressed;

		private GamePadState previousState;

		public GamePadHandler()
		{
			IsEnabled = GamePad.GetState(PlayerIndex.One).IsConnected;
			GamePadState = new Dictionary<Buttons, InputState>();

			// Map all button types of the gamepad per default
			foreach (int i in Enum.GetValues(typeof(Buttons)))
			{
				GamePadState.Add((Buttons)i, new InputState());
			}

			previousState = GamePad.GetState(PlayerIndex.One);
		}

		public void Update()
		{
			if (!IsEnabled)
			{
				return;
			}

			var currentState = GamePad.GetState(PlayerIndex.One);

			foreach (var pair in GamePadState)
			{
				pair.Value.Clear();

				// The button is pressed
				if (currentState.IsButtonDown(pair.Key))
				{
					GamePadState[pair.Key].Pressed = true;

					if (previousState.IsButtonUp(pair.Key))
					{
						GamePadState[pair.Key].WasJustPressed = true;
					}
				}
				// The button is released
				else
				{
					GamePadState[pair.Key].Pressed = false;

					if (previousState.IsButtonDown(pair.Key))
					{
						GamePadState[pair.Key].WasJustReleased = true;
					}
				}

				// Set the analoge value if the button provides it
				switch (pair.Key)
				{
					case Buttons.LeftThumbstickUp:
					case Buttons.LeftThumbstickDown:
						GamePadState[pair.Key].Value = currentState.ThumbSticks.Left.Y;
						break;

					case Buttons.LeftThumbstickLeft:
					case Buttons.LeftThumbstickRight:
						GamePadState[pair.Key].Value = currentState.ThumbSticks.Left.X;
						break;

					case Buttons.RightThumbstickUp:
					case Buttons.RightThumbstickDown:
						GamePadState[pair.Key].Value = currentState.ThumbSticks.Right.Y;
						break;

					case Buttons.RightThumbstickLeft:
					case Buttons.RightThumbstickRight:
						GamePadState[pair.Key].Value = currentState.ThumbSticks.Right.X;
						break;

					case Buttons.LeftTrigger:
						GamePadState[pair.Key].Value = currentState.Triggers.Left;
						break;

					case Buttons.RightTrigger:
						GamePadState[pair.Key].Value = currentState.Triggers.Right;
						break;
				}

				// Always display values in non-negative ranges
				if (GamePadState[pair.Key].Value < 0)
				{
					GamePadState[pair.Key].Value = -GamePadState[pair.Key].Value;
				}
			}

			previousState = currentState;
		}
	}
}