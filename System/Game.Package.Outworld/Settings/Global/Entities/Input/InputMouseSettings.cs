using System.Collections.Generic;
using Framework.Core.Contexts.Input;
using Microsoft.Xna.Framework.Input;

namespace Outworld.Settings.Global
{
	public class InputMouseSettings
	{
		public Dictionary<MouseInputType, Buttons> Mappings { get; set; }

		public InputMouseSettings()
		{
			Mappings = new Dictionary<MouseInputType, Buttons>();
		}
	}
}