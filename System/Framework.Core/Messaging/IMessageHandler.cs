using System.Collections.Generic;

namespace Framework.Core.Messaging
{
	public interface IMessageHandler
	{
		Dictionary<string, List<IMessage>> MessageGroups { get; set; }

		void AddMessage(string id, IMessage message);
		void Clear(string id);
		void Clear();
	}
}