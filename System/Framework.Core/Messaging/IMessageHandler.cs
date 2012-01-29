using System.Collections.Generic;

namespace Framework.Core.Messaging
{
	/// <summary>
	/// Handles lists of messages ordered by logical groups which can be used for communication between objects without coupling them together directly
	/// </summary>
	public interface IMessageHandler
	{
		Dictionary<int, List<IMessage>> MessageGroups { get; set; }

		void AddMessage(int groupId, IMessage message);
		List<T> GetMessages<T>(int groupId);
		void Clear(int groupId);
		void Clear();
	}
}