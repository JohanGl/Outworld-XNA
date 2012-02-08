using System.Collections.Generic;
using Framework.Network.Messages;

namespace Game.Network.Servers.Helpers
{
	public interface IMessageRecorder
	{
		void Record(List<Message> messages);
	}
}