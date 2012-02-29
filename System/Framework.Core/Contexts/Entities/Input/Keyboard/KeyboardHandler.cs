using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Framework.Core.Contexts.Input
{
	public class KeyboardHandler
	{
		public bool IsEnabled { get; private set; }
		public Dictionary<Keys, InputState> KeyboardState;
		public event EventHandler<KeyboardInputEventArgs> KeyPressed;

		private KeyboardState previousState;
		private KeyboardBuffer keyboardBuffer;

		public KeyboardHandler(GameContext context)
		{
			IsEnabled = Environment.OSVersion.Platform != PlatformID.Xbox;
			KeyboardState = new Dictionary<Keys, InputState>();

			if (IsEnabled)
			{
				keyboardBuffer = new KeyboardBuffer(context.Game.Window.Handle);
				keyboardBuffer.KeyPress += keyboardBuffer_KeyPress;
				keyboardBuffer.KeyDown += keyboardBuffer_KeyDown;
			}
		}

		public void Update()
		{
			if (!IsEnabled)
			{
				return;
			}

			var currentState = Keyboard.GetState();

			// Update all mapped keys
			foreach (var pair in KeyboardState)
			{
				pair.Value.Clear();
				var key = pair.Key;

				// The key is pressed
				if (currentState.IsKeyDown(key))
				{
					pair.Value.Pressed = true;
					pair.Value.Value = 1;

					// If the key was just pressed
					if (!previousState.IsKeyDown(key))
					{
						pair.Value.WasJustPressed = true;
					}
				}
				// The key is released
				else
				{
					// If the key was just released
					if (previousState.IsKeyDown(key))
					{
						pair.Value.WasJustReleased = true;
					}
				}
			}

			// Store the keyboard state
			previousState = currentState;
		}

		public void AddMapping(Keys key)
		{
			KeyboardState.Add(key, new InputState());
		}

		public void ClearMappings()
		{
			KeyboardState.Clear();
		}

		private void keyboardBuffer_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (KeyPressed != null)
			{
				var args = new KeyboardInputEventArgs
				{
					Value = e.KeyChar,
					IsShiftDown = keyboardBuffer.IsShiftDown,
					IsControlDown = keyboardBuffer.IsControlDown
				};

				KeyPressed(this, args);
			}
		}

		private void keyboardBuffer_KeyDown(object sender, KeyEventArgs e)
		{
			if (KeyPressed != null)
			{
				var args = new KeyboardInputEventArgs
				{
					IsShiftDown = keyboardBuffer.IsShiftDown,
					IsControlDown = keyboardBuffer.IsControlDown
				};

				switch ((Keys)e.KeyValue)
				{
					case Keys.Home:
					case Keys.End:
					case Keys.Up:
					case Keys.Down:
					case Keys.Left:
					case Keys.Right:
					case Keys.Delete:
					case Keys.LeftShift:
					case Keys.RightShift:
					case Keys.LeftControl:
					case Keys.RightControl:
						args.Key = (Keys)e.KeyValue;
						break;

					default:
						return;
				}

				KeyPressed(this, args);
			}
		}
	}
}