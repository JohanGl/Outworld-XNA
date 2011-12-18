using System.Collections.Generic;

namespace Framework.Core.Messaging
{
	public class DefaultMessageHandler : IMessageHandler
	{
		public Dictionary<string, List<IMessage>> MessageGroups { get; set; }

		public DefaultMessageHandler()
		{
			MessageGroups = new Dictionary<string, List<IMessage>>();
		}

		public void AddMessage(string id, IMessage message)
		{
			if (!MessageGroups.ContainsKey(id))
			{
				MessageGroups.Add(id, new List<IMessage>());
			}

			MessageGroups[id].Add(message);
		}

		public void Clear(string id)
		{
			MessageGroups.Remove(id);
		}

		public void Clear()
		{
			MessageGroups.Clear();
		}
	}
}