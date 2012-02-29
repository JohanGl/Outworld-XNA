using Framework.Core.Contexts.Input;

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
		public GamePadHandler GamePad { get; private set; }
		public BindingsHandler Bindings { get; private set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public InputContext(GameContext context)
		{
			// Initialize the mouse
			Mouse = new MouseHandler(context);

			// Initialize the keyboard
			Keyboard = new KeyboardHandler(context);

			// Initialize the gamepad
			GamePad = new GamePadHandler();

			// Initialize the bindings handler
			Bindings = new BindingsHandler(Mouse, Keyboard, GamePad);
		}

		/// <summary>
		/// Updates the current state of the different controller types
		/// </summary>
		public void Update()
		{
			Mouse.Update();
			Keyboard.Update();
			GamePad.Update();

			Bindings.Update();
		}
	}
}