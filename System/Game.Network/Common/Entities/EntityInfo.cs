using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Network.Common
{
	public class EntityInfo
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

		public List<EntityEvent> GetRecentEvents(float currentTimeStamp, float seconds = 2f)
		{
			var result = new List<EntityEvent>();

			// Subtract the clients local time to get an accurate measurement
			currentTimeStamp += RemoteTimeOffset;

			if (Events.Count > 0)
			{
				// Traverse backwards in time since the last events are the most recent
				for (int i = Events.Count - 1; i >= 0; i--)
				{
					// Get all events within the specified timeframe
					if (Events[i].TimeStamp > currentTimeStamp - seconds)
					{
						result.Add(Events[i]);
						System.Diagnostics.Debug.WriteLine(string.Format("Sending: TimeStamp: {0}, Type: {1}, Server: {2}, Server -2: {3}", Events[i].TimeStamp, Events[i].Type, currentTimeStamp, currentTimeStamp - seconds));
					}
					// No more events within the timespan
					else
					{
						//System.Diagnostics.Debug.WriteLine(string.Format("Failed event: TimeStamp: {0}, Type: {1}, Server: {2}", Events[i].TimeStamp, Events[i].Type, currentTimeStamp - seconds));
						break;
					}
				}
			}

			return result;
		}

		public bool IsWithinViewDistanceOf(EntityInfo otherEntity)
		{
			return Vector3.Distance(CurrentSpatial.Position, otherEntity.CurrentSpatial.Position) < 75;
		}

		public EntityInfo(ushort id, long serverId)
		{
			Id = id;
			ServerId = serverId;
			SpatialData = new List<EntitySpatial>(110);
			Events = new List<EntityEvent>();
			Timeout = int.MaxValue;
		}
	}
}