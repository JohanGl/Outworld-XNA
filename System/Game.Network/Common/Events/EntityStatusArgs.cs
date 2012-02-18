using System;

namespace Game.Network.Common
{
	public class EntityStatusArgs : EventArgs
	{
		public ushort Id;
		public long ServerId;
		public EntityType Type;
		public EntityStatusType StatusType;
	}
}