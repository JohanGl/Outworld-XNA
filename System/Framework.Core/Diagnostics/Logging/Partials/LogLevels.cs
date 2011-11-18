namespace Framework.Core.Diagnostics.Logging
{
	public enum LogLevel
	{
		Debug,
		Info,
		Warning,
		Error,
		Fatal
	}

	public static partial class Logger
	{
		public class LogLevels
		{
			public static LogLevel[] Default
			{
				get
				{
					return new[]
					{
						LogLevel.Warning,
						LogLevel.Error,
						LogLevel.Fatal
					};
				}
			}

			public static LogLevel[] Verbose
			{
				get
				{
					return new[]
					{
						LogLevel.Debug,
						LogLevel.Info,
						LogLevel.Warning,
						LogLevel.Error,
						LogLevel.Fatal
					};
				}
			}

			public static LogLevel[] Adaptive
			{
				get
				{
#if DEBUG
					return Verbose;
#else
					return Default;
#endif
				}
			}
		}
	}
}