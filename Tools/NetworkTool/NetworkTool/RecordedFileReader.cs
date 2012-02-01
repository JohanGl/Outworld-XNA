using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace NetworkTool
{
	public class RecordedFileReader
	{
		private string fileName;

		public RecordedFileReader(string fileName)
		{
			this.fileName = fileName;
		}

		public List<RecordedMessage> GetRecording()
		{
			var fileContent = File.ReadAllText(fileName);

			var serializer = new XmlSerializer(typeof(List<RecordedMessage>));
			var stringReader = new StringReader(fileContent);
			var result = (List<RecordedMessage>)serializer.Deserialize(stringReader);

			return result;
		}
	}
}