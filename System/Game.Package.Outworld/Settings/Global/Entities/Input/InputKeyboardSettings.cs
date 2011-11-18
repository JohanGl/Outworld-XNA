using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Outworld.Settings.Global
{
	public class InputKeyboardSettings
	{
		public Dictionary<Keys, Buttons> Mappings { get; set; }

		public InputKeyboardSettings()
		{
			Mappings = new Dictionary<Keys, Buttons>();
		}
	}
}