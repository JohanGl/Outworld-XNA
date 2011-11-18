using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

// Button types: http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.buttons.aspx

namespace Framework.Core.Contexts
{
	/// <summary>
	/// Handles input from gamepads, keyboard and mouse.
	/// Make sure to map keyboard and mouse buttons to the gamepad by using the methods AddKeyboardMapping/AddMouseMapping
	/// since the gamepad state is the one you should process using the State property when reading input data.
	/// </summary>
	public class InputContext
	{
		public MouseHandler Mouse { get; private set; }
		public KeyboardHandler Keyboard { get; private set; }

		/// <summary>
		/// Gets or sets the input states of the current controller
		/// </summary>
		public Dictionary<Buttons, InputState> GamePadState;

		/// <summary>
		/// Gets the flag indicating whether the user has a connected gamepad or not
		/// </summary>
		public bool HasGamePad { get; private set; }

		/// <summary>
		/// Stores a list of all available buttons so that we can iterate it easily
		/// </summary>
		private List<Buttons> buttonsList;

		private GamePadState previousGamePadState;

		/// <summary>
		/// Default constructor
		/// </summary>
		public InputContext(GameContext context)
		{
			// Initialize the mouse
			Mouse = new MouseHandler(context);

			// Initialize the keyboard
			Keyboard = new KeyboardHandler(context);

			GamePadState = new Dictionary<Buttons, InputState>();
			buttonsList = new List<Buttons>();

			// Map all button types of the gamepad per default
			foreach (int i in Enum.GetValues(typeof(Buttons)))
			{
				buttonsList.Add((Buttons)i);
				GamePadState.Add((Buttons)i, new InputState());
			}

			// Initialize the controller type availability states
			HasGamePad = GamePad.GetState(PlayerIndex.One).IsConnected;

			// Initialize the previous keyboard states
			previousGamePadState = GamePad.GetState(PlayerIndex.One);
		}

		/// <summary>
		/// Updates the current state of the different controller types
		/// </summary>
		public void Update()
		{
			// Reset all states
			foreach (var button in buttonsList)
			{
				GamePadState[button].Clear();
			}

			if (HasGamePad)
			{
				HandleGamePad();
			}

			Mouse.Update();
			Keyboard.Update();
		}

		private void HandleGamePad()
		{
			var currentGamePadState = GamePad.GetState(PlayerIndex.One);

			foreach (var button in buttonsList)
			{
				// The button is pressed
				if (currentGamePadState.IsButtonDown(button))
				{
					GamePadState[button].Pressed = true;

					if (previousGamePadState.IsButtonUp(button))
					{
						GamePadState[button].WasJustPressed = true;
					}
				}
				// The button is released
				else
				{
					GamePadState[button].Pressed = false;

					if (previousGamePadState.IsButtonDown(button))
					{
						GamePadState[button].WasJustReleased = true;
					}
				}

				// Set the analoge value if the button provides it
				switch (button)
				{
					case Buttons.LeftThumbstickUp:
					case Buttons.LeftThumbstickDown:
						GamePadState[button].Value = currentGamePadState.ThumbSticks.Left.Y;
						break;

					case Buttons.LeftThumbstickLeft:
					case Buttons.LeftThumbstickRight:
						GamePadState[button].Value = currentGamePadState.ThumbSticks.Left.X;
						break;

					case Buttons.RightThumbstickUp:
					case Buttons.RightThumbstickDown:
						GamePadState[button].Value = currentGamePadState.ThumbSticks.Right.Y;
						break;

					case Buttons.RightThumbstickLeft:
					case Buttons.RightThumbstickRight:
						GamePadState[button].Value = currentGamePadState.ThumbSticks.Right.X;
						break;

					case Buttons.LeftTrigger:
						GamePadState[button].Value = currentGamePadState.Triggers.Left;
						break;

					case Buttons.RightTrigger:
						GamePadState[button].Value = currentGamePadState.Triggers.Right;
						break;
				}

				// Always display values in non-negative ranges
				if (GamePadState[button].Value < 0)
				{
					GamePadState[button].Value = -GamePadState[button].Value;
				}
			}

			previousGamePadState = currentGamePadState;
		}
	}
}