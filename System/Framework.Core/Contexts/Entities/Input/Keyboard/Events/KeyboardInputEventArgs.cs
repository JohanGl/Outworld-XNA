using System;
using Microsoft.Xna.Framework.Input;

namespace Framework.Core.Contexts.Input
{
	public class KeyboardInputEventArgs : EventArgs
	{
		public Keys? Key;
		public int Value;
		public bool IsShiftDown;
		public bool IsControlDown;
	}
}