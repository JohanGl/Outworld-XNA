using Framework.Core.Common;
using Game.World.Terrains.Generators.Noise.Managers;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Tiles;

namespace Game.World.Terrains.Generators.Areas.Tiles
{
	public class DebugGenerator : IAreaTilesGenerator
	{
		private NoiseManager noiseManager;
		private AreaHeightMapCache heightMapCache;
		private byte[] surface;

		public DebugGenerator(NoiseManager noiseManager)
		{
			this.noiseManager = noiseManager;

			heightMapCache = new AreaHeightMapCache();
			surface = new byte[Area.LevelSize];
		}

		public void GenerateSurface(ref Area area)
		{
			for (int y = 0; y < Area.Size.Y; y++)
			{
				int currentY = y * Area.LevelSize;

				for (int z = 0; z < Area.Size.Z; z++)
				{
					int currentZ = z * Area.Size.X;

					for (int x = 0; x < Area.Size.X; x++)
					{
						int currentIndex = currentY + currentZ + x;

						area.Info.Tiles[currentIndex].Type = TileType.Empty;

						//if (y == Area.Size.Y - 1)
						//{
						//    area.Info.Tiles[currentIndex].Type = TileType.Stone;
						//}
						//else if (y == Area.Size.Y - 2 && x == 16 && z == 16)
						//{
						//    area.Info.Tiles[currentIndex].Type = TileType.Stone;
						//}
						//else if (y > 20 && x == 17 && z == 16)
						//{
						//    area.Info.Tiles[currentIndex].Type = TileType.Stone;
						//}
						//else if (y > 20 && x == 15 && z == 16)
						//{
						//    area.Info.Tiles[currentIndex].Type = TileType.Stone;
						//}
						//else if (y > 23 && x == 16 && z == 17)
						//{
						//    area.Info.Tiles[currentIndex].Type = TileType.Stone;
						//}
					}
				}
			}
		}

		public void GenerateUnderground(ref Area area)
		{
			for (int i = 0; i < area.Info.Tiles.Length; i++)
			{
				area.Info.Tiles[i].Type = TileType.Mud;
			}
		}

		public int GetHeight(Vector2i location, int x, int z)
		{
			return 0;
		}

		public void TrimHeight(ref Area area, int x, int z, int height)
		{
			int index = (z * Area.Size.X) + ((Area.Size.Y - 1) * Area.LevelSize) + x;

			for (int y = 0; y < Area.Size.Y; y++)
			{
				// Above the surface
				if (y >= height)
				{
					area.Info.Tiles[index].Type = TileType.Empty;
				}
				// At the surface
				else if (y == height - 1)
				{
					area.Info.Tiles[index].Type = GetSurfaceTileType(area.Info.Location, x, y, z);
				}
				// Below the surface
				else
				{
					area.Info.Tiles[index].Type = TileType.Stone;
				}

				index -= Area.LevelSize;
			}
		}

		public void ExpandHeight(ref Area area, int x, int z, int height)
		{
			if (height < 1)
			{
				return;
			}

			int index = (z * Area.Size.X) + ((Area.Size.Y - 1) * Area.LevelSize) + x;

			for (int y = 0; y < Area.Size.Y; y++)
			{
				if (y < height || height == Area.Size.Y - 1)
				{
					area.Info.Tiles[index].Type = TileType.Stone;
				}
				else if (y == height)
				{
					area.Info.Tiles[index].Type = GetSurfaceTileType(area.Info.Location, x, y, z);
				}

				index -= Area.LevelSize;
			}
		}

		public void ClearCache()
		{
			heightMapCache.HeightMaps.Clear();
		}

		private TileType GetSurfaceTileType(Vector3i location, int tileX, int tileY, int tileZ)
		{
			return TileType.Stone;
		}
	}
}