using System;
using System.IO;

namespace Framework.Core.Diagnostics.Logging
{
	public class FileOutputSource : IOutputSource
	{
		public bool ApplySignature { get; set; }

		private readonly string fileName;
		private static readonly object writeLock = new object();

		public FileOutputSource(string fileName, bool emptyFile = true)
		{
			this.fileName = fileName;

			if (emptyFile && File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			ApplySignature = true;
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