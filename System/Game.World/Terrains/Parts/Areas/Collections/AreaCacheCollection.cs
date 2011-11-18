using System.Collections.Generic;
using System.Collections.ObjectModel;
using Framework.Core.Common;

namespace Game.World.Terrains.Parts.Areas.Collections
{
	/// <summary>
	/// Defines a read only collection of areas which can only be manipulated through a set of functions and provides an optimized lookup function
	/// </summary>
	public class AreaCacheCollection
	{
		private readonly AreaCollection areaCollection;

		public ReadOnlyCollection<Area> Areas
		{
			get
			{
				return areaCollection.Areas;
			}
		}

		public AreaCacheCollection()
		{
			areaCollection = new AreaCollection();
		}

		public void Add(Area area)
		{
			lock (areaCollection.CollectionLock)
			{
				areaCollection.Add(area);
			}
		}

		public void AddRange(List<Area> areas)
		{
			lock (areaCollection.CollectionLock)
			{
				areaCollection.AddRange(areas);
			}
		}

		public void Remove(Area area)
		{
			lock (areaCollection.CollectionLock)
			{
				areaCollection.Remove(area);
			}
		}

		public void RemoveRange(List<Area> areas)
		{
			lock (areaCollection.CollectionLock)
			{
				areaCollection.RemoveRange(areas);
			}
		}

		public void Clear()
		{
			lock (areaCollection.CollectionLock)
			{
				areaCollection.Clear();
			}
		}

		public Area GetAreaAt(int x, int y, int z)
		{
			lock (areaCollection.CollectionLock)
			{
				return areaCollection.GetAreaAt(x, y, z);
			}
		}

		public void RemoveExpiredAreas(Vector3i location, Vector3i viewDistance)
		{
			lock (areaCollection.CollectionLock)
			{
				var expiredAreas = new List<Area>();

				Area area;

				// Loop through all areas to find expired ones
				for (int i = 0; i < areaCollection.Areas.Count; i++)
				{
					area = areaCollection.Areas[i];

					if (area.Info.Location.X >= location.X)
					{
						if (area.Info.Location.X - location.X > viewDistance.X + 2)
						{
							expiredAreas.Add(area);
						}
					}
					else
					{
						if (location.X - area.Info.Location.X > viewDistance.X + 2)
						{
							expiredAreas.Add(area);
						}
					}

					if (area.Info.Location.Y >= location.Y)
					{
						if (area.Info.Location.Y - location.Y > viewDistance.Y + 2)
						{
							expiredAreas.Add(area);
						}
					}
					else
					{
						if (location.Y - area.Info.Location.Y > viewDistance.Y + 2)
						{
							expiredAreas.Add(area);
						}
					}

					if (area.Info.Location.Z >= location.Z)
					{
						if (area.Info.Location.Z - location.Z > viewDistance.Z + 2)
						{
							expiredAreas.Add(area);
						}
					}
					else
					{
						if (location.Z - area.Info.Location.Z > viewDistance.Z + 2)
						{
							expiredAreas.Add(area);
						}
					}
				}

				// Remove all expired areas
				RemoveRange(expiredAreas);
			}
		}
	}
}