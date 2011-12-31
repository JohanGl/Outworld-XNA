using System;

namespace Framework.Gui
{
	public class ElementStateChangeArgs : EventArgs
	{
		public ElementState State { get; set; }
	}

	public enum ElementState
	{
		Focused,
		Highlighted
	}
}