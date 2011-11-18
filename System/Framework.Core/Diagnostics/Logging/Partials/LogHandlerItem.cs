using System.Collections.Generic;

namespace Framework.Core.Diagnostics.Logging
{
	public static partial class Logger
	{
		private class LogHandlerItem
		{
			public List<LogLevel> Levels { get; set; }
			public bool IsMuted { get; set; }

			public LogHandlerItem()
			{
				Levels = new List<LogLevel>();
			}

			public LogHandlerItem(List<LogLevel> levels)
			{
				Levels = levels;
			}
		}
	}
}