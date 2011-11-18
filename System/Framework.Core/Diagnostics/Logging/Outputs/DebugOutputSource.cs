namespace Framework.Core.Diagnostics.Logging
{
	public class DebugOutputSource : IOutputSource
	{
		public void Write(string text)
		{
			System.Diagnostics.Debug.WriteLine(text);
		}
	}
}