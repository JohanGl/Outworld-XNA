using System.Collections.Generic;

namespace Framework.Core.Messaging
{
	/// <summary>
	/// Handles lists of messages ordered by logical groups which can be used for communication between objects without coupling them together directly
	/// </summary>
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

		public List<T> GetMessages<T>(string id)
		{
			var result = new List<T>();

			if (MessageGroups.ContainsKey(id))
			{
				for (int i = 0; i < MessageGroups[id].Count; i++)
				{
					result.Add((T)MessageGroups[id][i]);
				}
			}

			return result;
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