using System;
using System.IO;

namespace Framework.Core.Diagnostics.Logging
{
	public class FileOutputSource : IOutputSource
	{
		private readonly string fileName;
		private static readonly object writeLock = new object();

		public FileOutputSource(string fileName, bool emptyFile = true)
		{
			this.fileName = fileName;

			if (emptyFile && File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		public void Write(string text)
		{
			lock (writeLock)
			{
				File.AppendAllText(fileName, text + Environment.NewLine);
			}
		}
	}
}