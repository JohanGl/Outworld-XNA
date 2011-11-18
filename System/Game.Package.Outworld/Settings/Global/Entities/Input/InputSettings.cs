namespace Outworld.Settings.Global
{
	public class InputSettings
	{
		public InputKeyboardSettings Keyboard { get; set; }
		public InputMouseSettings Mouse { get; set; }
		public InputGamePadSettings GamePad { get; set; }

		public InputSettings()
		{
			Keyboard = new InputKeyboardSettings();
			Mouse = new InputMouseSettings();
			GamePad = new InputGamePadSettings();
		}
	}
}