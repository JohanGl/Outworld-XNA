using System.Collections.Generic;

namespace Framework.Core.Messaging
{
	/// <summary>
	/// Handles lists of messages ordered by logical groups which can be used for communication between objects without coupling them together directly
	/// </summary>
	public interface IMessageHandler
	{
		bool Contains(int groupId);
		bool HasMessages(int groupId);
		void AddMessage(int groupId, object message);
		void RemoveMessage(int groupId, object message);
		List<T> GetMessages<T>(int groupId);
		void Clear(int? groupId = null);
	}
}