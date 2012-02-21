using System.Collections.Generic;

namespace Game.Network.Common
{
	public class ServerEntity
	{
		public ushort Id { get; private set; }
		public long ServerId { get; private set; }
		public EntityType Type;

		public float RemoteTimeOffset { get; set; }

		public int Timeout;
		public List<EntitySpatial> SpatialData { get; set; }
		
		public List<EntityEvent> Events { get; set; }

		public EntitySpatial CurrentSpatial
		{
			get
			{
				if (SpatialData.Count > 0)
				{
					return SpatialData[SpatialData.Count - 1];
				}

				return new EntitySpatial();
			}
		}

		public ServerEntity(ushort id, long serverId)
		{
			Id = id;
			ServerId = serverId;
			SpatialData = new List<EntitySpatial>(110);
			Events = new List<EntityEvent>();
			Timeout = int.MaxValue;
		}
	}
}