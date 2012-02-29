using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Framework.Core.Contexts.Input
{
	public class BindingsHandler
	{
		private MouseHandler mouseHandler;
		private KeyboardHandler keyboardHandler;
		private GamePadHandler gamePadHandler;
		public Dictionary<int, InputState> Binding { get; private set; }

		private Dictionary<int, List<Keys>> keyboardBindings;
		private Dictionary<int, List<MouseInputType>> mouseBindings;
		private Dictionary<int, List<Buttons>> gamePadBindings;

		public BindingsHandler(MouseHandler mouseHandler, KeyboardHandler keyboardHandler, GamePadHandler gamePadHandler)
		{
			this.mouseHandler = mouseHandler;
			this.keyboardHandler = keyboardHandler;
			this.gamePadHandler = gamePadHandler;

			Binding = new Dictionary<int, InputState>();
			keyboardBindings = new Dictionary<int, List<Keys>>();
			mouseBindings = new Dictionary<int, List<MouseInputType>>();
			gamePadBindings = new Dictionary<int, List<Buttons>>();
		}

		public void AddKeyboardBinding(Keys key, int commandId)
		{
			if (!keyboardHandler.KeyboardState.ContainsKey(key))
			{
				return;
			}

			if (!keyboardBindings.ContainsKey(commandId))
			{
				keyboardBindings.Add(commandId, new List<Keys>());
			}

			keyboardBindings[commandId].Add(key);
			UpdateBindings(commandId);
		}

		public void AddMouseBinding(MouseInputType type, int commandId)
		{
			if (!mouseHandler.MouseState.ContainsKey(type))
			{
				return;
			}

			if (!mouseBindings.ContainsKey(commandId))
			{
				mouseBindings.Add(commandId, new List<MouseInputType>());
			}

			mouseBindings[commandId].Add(type);
			UpdateBindings(commandId);
		}

		public void AddGamePadBinding(Buttons button, int commandId)
		{
			if (!gamePadHandler.GamePadState.ContainsKey(button))
			{
				return;
			}

			if (!gamePadBindings.ContainsKey(commandId))
			{
				gamePadBindings.Add(commandId, new List<Buttons>());
			}

			gamePadBindings[commandId].Add(button);
			UpdateBindings(commandId);
		}

		private void UpdateBindings(int commandId)
		{
			if (!Binding.ContainsKey(commandId))
			{
				Binding.Add(commandId, new InputState());
			}
		}

		public void Update()
		{
			foreach (var pair in Binding)
			{
				// Clear the current state
				pair.Value.Clear();

				bool updatedBinding = false;

				// Process keyboard bindings
				if (keyboardBindings.ContainsKey(pair.Key))
				{
					for (int i = 0; i < keyboardBindings[pair.Key].Count; i++)
					{
						var key = keyboardBindings[pair.Key][i];
						var state = keyboardHandler.KeyboardState[key];

						if (UpdatedInputState(state, pair.Value))
						{
							updatedBinding = true;
							break;
						}
					}
				}

				// Process mouse bindings
				if (!updatedBinding && mouseBindings.ContainsKey(pair.Key))
				{
					for (int i = 0; i < mouseBindings[pair.Key].Count; i++)
					{
						var type = mouseBindings[pair.Key][i];
						var state = mouseHandler.MouseState[type];

						if (UpdatedInputState(state, pair.Value))
						{
							updatedBinding = true;
							break;
						}
					}
				}

				// Process gamepad bindings
				if (!updatedBinding && gamePadBindings.ContainsKey(pair.Key))
				{
					for (int i = 0; i < gamePadBindings[pair.Key].Count; i++)
					{
						var button = gamePadBindings[pair.Key][i];
						var state = gamePadHandler.GamePadState[button];

						if (UpdatedInputState(state, pair.Value))
						{
							break;
						}
					}
				}
			}
		}

		private bool UpdatedInputState(InputState from, InputState to)
		{
			if (from.Pressed || from.WasJustPressed || from.WasJustReleased || from.Value != 0)
			{
				to.Pressed = from.Pressed;
				to.WasJustPressed = from.WasJustPressed;
				to.WasJustReleased = from.WasJustReleased;
				to.Value = from.Value;

				return true;
			}

			return false;
		}

		public void Clear()
		{
			Binding.Clear();
			mouseBindings.Clear();
			keyboardBindings.Clear();
			gamePadBindings.Clear();
		}
	}
}