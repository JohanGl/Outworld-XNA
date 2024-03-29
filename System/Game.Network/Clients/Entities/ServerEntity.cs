using Game.Network.Common;
using Microsoft.Xna.Framework;

namespace Game.Network.Clients
{
	public class ServerEntity
	{
		public ushort Id;
		public EntityType Type;

		public Vector3 Position;
		public Vector3 Velocity;
		public Vector3 Angle;
	
		public byte Animation;
		public byte PreviousAnimation;
	}
}