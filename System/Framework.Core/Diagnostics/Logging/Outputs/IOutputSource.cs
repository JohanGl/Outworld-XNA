namespace Framework.Core.Diagnostics.Logging
{
	public interface IOutputSource
	{
		bool ApplySignature { get; set; }
		void Write(string text);
	}
}