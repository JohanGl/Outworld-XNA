using System.Collections.Generic;

namespace Framework.Core.Messaging
{
	/// <summary>
	/// Handles lists of messages ordered by logical groups which can be used for communication between objects without coupling them together directly
	/// </summary>
	public class DefaultMessageHandler : IMessageHandler
	{
		private Dictionary<int, List<object>> MessageGroups { get; set; }

		public bool Contains(int groupId)
		{
			return MessageGroups.ContainsKey(groupId);
		}

		public bool HasMessages(int groupId)
		{
			if (!Contains(groupId))
			{
				return false;
			}

			return MessageGroups[groupId].Count > 0;
		}

		public DefaultMessageHandler()
		{
			MessageGroups = new Dictionary<int, List<object>>();
		}

		public void AddMessage(int groupId, object message)
		{
			if (!Contains(groupId))
			{
				MessageGroups.Add(groupId, new List<object>());
			}

			MessageGroups[groupId].Add(message);
		}

		public void RemoveMessage(int groupId, object message)
		{
			if (Contains(groupId))
			{
				MessageGroups[groupId].Remove(message);
			}
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

		public void Clear(int? groupId)
		{
			if (groupId.HasValue)
			{
				MessageGroups.Remove(groupId.Value);
			}
			else
			{
				MessageGroups.Clear();
			}
		}
	}
}