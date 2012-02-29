using System.Collections.Generic;
using System.Collections.ObjectModel;
using Framework.Core.Common;

namespace Game.World.Terrains.Parts.Areas.Collections
{
	/// <summary>
	/// Defines a read only collection of areas which can only be manipulated through a set of functions and provides an optimized lookup function
	/// </summary>
	public class AreaCollection
	{
		private List<Area> areas;
		public readonly object CollectionLock = new object();

		public ReadOnlyCollection<Area> Areas
		{
			get
			{
				lock (CollectionLock)
				{
					return areas.AsReadOnly();
				}
			}
		}

		private Dictionary<string, Area> lookup;

		public AreaCollection()
		{
			areas = new List<Area>();
			lookup = new Dictionary<string, Area>();
		}

		public void Add(Area area)
		{
			lock (CollectionLock)
			{
				string key = area.Info.LocationId;

				if (lookup.ContainsKey(key))
				{
					areas.Remove(lookup[key]);
					lookup.Remove(key);
				}

				areas.Add(area);
				lookup.Add(key, area);
			}
		}

		public void AddRange(List<Area> areas)
		{
			lock (CollectionLock)
			{
				for (int i = 0; i < areas.Count; i++)
				{
					Add(areas[i]);
				}
			}
		}

		public void Remove(Area area)
		{
			lock (CollectionLock)
			{
				string key = area.Info.LocationId;

				if (lookup.ContainsKey(key))
				{
					lookup.Remove(key);
					areas.Remove(area);
				}
			}
		}

		public void RemoveRange(List<Area> areas)
		{
			lock (CollectionLock)
			{
				for (int i = 0; i < areas.Count; i++)
				{
					Remove(areas[i]);				
				}
			}
		}

		public void Clear()
		{
			lock (CollectionLock)
			{
				lookup.Clear();
				areas.ForEach(p => p.Clear());
				areas.Clear();
			}
		}

		public Area GetAreaAt(Vector3i location)
		{
			return GetAreaAt(location.X, location.Y, location.Z);
		}

		public Area GetAreaAt(int x, int y, int z)
		{
			lock (CollectionLock)
			{
				string location = Vector3i.ConvertToString(x, y, z);

				if (lookup.ContainsKey(location))
				{
					return lookup[location];
				}

				return null;
			}
		}
	}
}