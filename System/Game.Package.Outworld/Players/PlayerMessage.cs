using Framework.Core.Messaging;
using Game.Network.Common;

namespace Game.Network.Clients
{
	public class PlayerMessage : IMessage
	{
		public ClientActionType Type;
	}
}