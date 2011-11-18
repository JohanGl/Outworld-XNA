namespace Framework.Core.Diagnostics.Logging
{
	public interface IOutputSource
	{
		void Write(string text);
	}
}