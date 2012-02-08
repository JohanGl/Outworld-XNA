namespace Framework.Core.Diagnostics.Logging
{
	public class DebugOutputSource : IOutputSource
	{
		public bool ApplySignature { get; set; }

		public DebugOutputSource()
		{
			ApplySignature = true;
		}

		public void Write(string text)
		{
			System.Diagnostics.Debug.WriteLine(text);
		}
	}
}