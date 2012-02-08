using System;
using System.Collections.Generic;
using Framework.Core.Diagnostics.Logging;
using Framework.Network.Messages;

namespace Game.Network.Servers.Helpers
{
	public class DefaultMessageRecorder : IMessageRecorder
	{
		public DefaultMessageRecorder()
		{
			Logger.RegisterLogLevelsFor<IMessageRecorder>(LogLevel.Debug);
			Logger.OverrideOutputFor<IMessageRecorder>(new FileOutputSource("server.txt") { ApplySignature = false });
		}

		public void Record(List<Message> messages)
		{
			if (messages == null || messages.Count == 0)
			{
				return;
			}

			for (int i = 0; i < messages.Count; i++)
			{
				var message = messages[i];

				// Ugly in code, nice in output :)
				string result = string.Format(
@"	<Message>
		<ClientId>{0}</ClientId>
		<Data>{1}</Data>
		<RemoteTimeOffset>{2}</RemoteTimeOffset>
		<Type>{3}</Type>
	</Message>", message.ClientId, BytesToString(message.Data), message.RemoteTimeOffset, message.Type);

				Logger.Log<IMessageRecorder>(LogLevel.Debug, result);
			}
		}

		private string BytesToString(byte[] data)
		{
			string bytes = "";

			if (data == null)
			{
				return bytes;
			}

			for (int j = 0; j < data.Length; j++)
			{
				bytes += Convert.ToInt16(data[j]).ToString();

				if (j < data.Length - 1)
				{
					bytes += ",";
				}
			}

			return bytes;
		}
	}
}