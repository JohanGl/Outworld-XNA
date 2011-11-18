using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Areas.Collections;

namespace Game.World.Terrains.Visibility
{
	public partial class TerrainVisibility
	{
		public class TerrainVisibilityStatistics
		{
			public int TotalTiles { get; private set; }
			public int TotalVisibleTiles { get; private set; }
			public int TotalFaces { get; private set; }
			public int TotalVisibleFaces { get; private set; }
			public int TotalAreas { get { return terrainVisibility.AreaCollection.Areas.Count; } }
			public int TotalCachedAreas { get { return cache.Areas.Count; } }

			private TerrainVisibility terrainVisibility;
			private AreaCacheCollection cache;

			public TerrainVisibilityStatistics(TerrainVisibility terrainVisibility, AreaCacheCollection cache)
			{
				this.terrainVisibility = terrainVisibility;
				this.cache = cache;
			}

			public void UpdateStatistics()
			{
				// Update the terrain statistics
				TotalTiles = Area.TotalTiles * terrainVisibility.AreaCollection.Areas.Count;
				TotalVisibleTiles = 0;
				TotalFaces = 0;
				TotalVisibleFaces = 0;

				foreach (var area in terrainVisibility.AreaCollection.Areas)
				{
					if (area.Info.IsEmpty)
					{
						continue;
					}

					TotalFaces += area.Model.TotalFaces;
					TotalVisibleFaces += area.Model.TotalVisibleFaces;

					foreach (var tile in area.Info.Tiles)
					{
						if (tile.Type != 0)
						{
							TotalVisibleTiles++;
						}
					}
				}
			}
		}
	}
}