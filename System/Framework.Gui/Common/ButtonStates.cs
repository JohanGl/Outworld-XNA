using System;
using Microsoft.Xna.Framework;

namespace Framework.Gui
{
	public struct ButtonStates
	{
		public Rectangle Default;
		public Rectangle Pressed;
		public Rectangle Focused;
		public Rectangle Highlighted;

		public void Validate()
		{
			if (Default == Rectangle.Empty)
			{
				throw new Exception("The variable Default must contain a value other than Rectangle.Empty");
			}

			if (Pressed == Rectangle.Empty)
			{
				Pressed = Default;
			}

			if (Focused == Rectangle.Empty)
			{
				Focused = Default;
			}

			if (Highlighted == Rectangle.Empty)
			{
				Highlighted = Default;
			}
		}
	}
}