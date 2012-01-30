using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Network.Common
{
	public class EntityInfo
	{
		public ushort Id;
		public EntityType Type;

		public int Timeout;
		public List<EntitySpatial> SpatialData { get; set; }
		public List<EntityEvent> Actions { get; set; }

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

		public List<EntityEvent> GetRecentActions(float currentTimeStamp)
		{
			var result = new List<EntityEvent>();

			if (Actions.Count > 0)
			{
				// Traverse backwards in time since the last actions are the most recent
				for (int i = Actions.Count - 1; i >= 0; i--)
				{
					// Get all actions within the last second
					if (Actions[i].TimeStamp > currentTimeStamp - 1.0f)
					{
						result.Add(Actions[i]);
					}
					// No more relevant actions left
					else
					{
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

		public EntityInfo()
		{
			SpatialData = new List<EntitySpatial>(110);
			Actions = new List<EntityEvent>(110);
			Timeout = int.MaxValue;
		}
	}
}