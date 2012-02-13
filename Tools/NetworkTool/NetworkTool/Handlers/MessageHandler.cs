using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Framework.Network.Messages;

namespace NetworkTool
{
	public class MessageHandler
	{
		public List<Message> GetRecordings(string fileName)
		{
			var document = XDocument.Load(fileName);

			var result = (from item in document.Descendants("Message")
						  select new Message
						  {
							  ClientId = GetClientId(item),
							  RemoteTimeOffset = GetRemoteTimeOffset(item),
							  Type = GetType(item),
							  Data = GetData(item),
							  DataRaw = item.Element("Data").Value
						  }).ToList();

			return result;
		}

		private long GetClientId(XElement item)
		{
			return long.Parse(item.Element("ClientId").Value);
		}

		private float GetRemoteTimeOffset(XElement item)
		{
			return float.Parse(item.Element("RemoteTimeOffset").Value);
		}

		private MessageType GetType(XElement item)
		{
			return (MessageType)Enum.Parse(typeof(MessageType), item.Element("Type").Value, true);
		}

		private byte[] GetData(XElement item)
		{
			string data = item.Element("Data").Value;

			if (string.IsNullOrEmpty(data))
			{
				return new byte[0];
			}

			var bytes = data.Split(',');

			var result = new byte[bytes.Length];

			for (int i = 0; i < bytes.Length; i++)
			{
				result[i] = byte.Parse(bytes[i]);
			}

			return result;
		}
	}
}