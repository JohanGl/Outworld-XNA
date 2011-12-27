using System.Collections.Generic;

namespace Framework.Core.Messaging
{
	/// <summary>
	/// Handles lists of messages ordered by logical groups which can be used for communication between objects without coupling them together directly
	/// </summary>
	public interface IMessageHandler
	{
		Dictionary<string, List<IMessage>> MessageGroups { get; set; }

		void AddMessage(string id, IMessage message);
		List<T> GetMessages<T>(string id);
		void Clear(string id);
		void Clear();
	}
}