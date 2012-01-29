using System.Collections.Generic;

namespace Framework.Core.Messaging
{
	/// <summary>
	/// Handles lists of messages ordered by logical groups which can be used for communication between objects without coupling them together directly
	/// </summary>
	public class DefaultMessageHandler : IMessageHandler
	{
		public Dictionary<int, List<IMessage>> MessageGroups { get; set; }

		public DefaultMessageHandler()
		{
			MessageGroups = new Dictionary<int, List<IMessage>>();
		}

		public void AddMessage(int groupId, IMessage message)
		{
			if (!MessageGroups.ContainsKey(groupId))
			{
				MessageGroups.Add(groupId, new List<IMessage>());
			}

			MessageGroups[groupId].Add(message);
		}

		public List<T> GetMessages<T>(int groupId)
		{
			var result = new List<T>();

			if (MessageGroups.ContainsKey(groupId))
			{
				for (int i = 0; i < MessageGroups[groupId].Count; i++)
				{
					result.Add((T)MessageGroups[groupId][i]);
				}
			}

			return result;
		}

		public void Clear(int groupId)
		{
			MessageGroups.Remove(groupId);
		}

		public void Clear()
		{
			MessageGroups.Clear();
		}
	}
}