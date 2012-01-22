using System;
using System.IO;
using System.Xml.Serialization;

namespace Framework.Core.Helpers
{
	public class ObjectDumper
	{
		public string ObjectToXml(object input)
		{
			string objectAsXmlString;

			var serializer = new XmlSerializer(input.GetType());

			using (var stringWriter = new StringWriter())
			{
				try
				{
					serializer.Serialize(stringWriter, input);
					objectAsXmlString = stringWriter.ToString();
				}
				catch (Exception e)
				{
					objectAsXmlString = e.ToString();
				}
			}

			return objectAsXmlString;
		}
	}
}