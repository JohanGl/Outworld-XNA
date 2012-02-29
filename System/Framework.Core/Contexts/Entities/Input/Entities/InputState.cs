namespace Framework.Core.Contexts.Input
{
	/// <summary>
	/// Defines the current state of a gamepad button
	/// </summary>
	public class InputState
	{
		public bool Pressed;
		public bool WasJustPressed;
		public bool WasJustReleased;
		public float Value;

		public void Clear()
		{
			Pressed = false;
			WasJustPressed = false;
			WasJustReleased = false;
			Value = 0;
		}
	}
}