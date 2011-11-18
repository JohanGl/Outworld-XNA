using System;

namespace Framework.Gui.Events
{
	public class TextBoxEventArgs : EventArgs
	{
		public string Text { get; set; }

		public TextBoxEventArgs()
		{
		}

		public TextBoxEventArgs(string text)
		{
			Text = text;
		}
	}
}
