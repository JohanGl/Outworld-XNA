using Framework.Core.Common;
using Game.World.Terrains.Generators.Noise.Managers;
using Game.World.Terrains.Generators.Noise.Managers.AreaResources;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Helpers;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Tiles;

namespace Game.World.Terrains.Generators.Areas.Tiles
{
	public class MountainsGenerator : IAreaTilesGenerator
	{
		private NoiseManager noiseManager;
		private AreaHeightMapCache heightMapCache;
		private byte[] surface;

		public MountainsGenerator(NoiseManager noiseManager)
		{
			this.noiseManager = noiseManager;

			heightMapCache = new AreaHeightMapCache();
			surface = new byte[Area.LevelSize];
		}

		public void GenerateSurface(ref Area area)
		{
			GenerateNoiseMountains(area.Info.Location.X, area.Info.Location.Z);

			int index = 0;

			// Down/up (vertically)
			for (int tileY = 0; tileY < Area.Size.Y; tileY++)
			{
				int currentTileY = (Area.Size.Y - (tileY + 1)) + (area.Info.Location.Y * Area.Size.Y);

				// In/out
				for (int tileZ = 0; tileZ < Area.Size.Z; tileZ++)
				{
					int currentZ = tileZ * Area.Size.Z;

					// Left/right (horizontally)
					for (int tileX = 0; tileX < Area.Size.X; tileX++)
					{
						int currentSurfaceIndex = currentZ + tileX;
						int currentSurfaceY = surface[currentSurfaceIndex];

						if (area.Info.Location.Y == 0)
						{
							if (currentTileY > currentSurfaceY)
							{
								area.Info.Tiles[index].Type = TileType.Empty;
							}
							else if (currentTileY > 0)
							{
								area.Info.Tiles[index].Type = GetSurfaceTileType(area.Info.Location, tileX, tileY, tileZ);
							}
							else
							{
								area.Info.Tiles[index].Type = TileType.Sand;
							}
						}
						else
						{
							// Above the ground?
							if (currentTileY > currentSurfaceY)
							{
								// Empty tile
								area.Info.Tiles[index].Type = TileType.Empty;
							}
							// At the exact ground level?
							else if (currentTileY == currentSurfaceY)
							{
								area.Info.Tiles[index].Type = GetSurfaceTileType(area.Info.Location, tileX, tileY, tileZ);
							}
							// Below the ground level
							else
							{
								area.Info.Tiles[index].Type = GetSurfaceTileType(area.Info.Location, tileX, tileY, tileZ);
							}
						}

						index++;
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
			if (!heightMapCache.HeightMaps.ContainsKey(location))
			{
				var area = new Area();
				area.Info.Location = new Vector3i(location.X, 0, location.Y);
				GenerateSurface(ref area);
			}

			return heightMapCache.HeightMaps[location][(z * Area.Size.X) + x];
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
			byte tileType = noiseManager.AreaResources.Generators[NoiseAreaResourceType.Default].GenerateSinglePoint(
				tileX + (location.X * Area.Size.X),
				tileY + (location.Y * Area.Size.Y),
				tileZ + (location.Z * Area.Size.Z));

			if (tileType > 127)
			{
				return TileType.Stone;
			}
			else
			{
				return TileType.Stone2;
			}
		}

		private void GenerateNoiseMountains(int x, int z)
		{
			var location = new Vector2i(x, z);

			if (heightMapCache.HeightMaps.ContainsKey(location))
			{
				surface = heightMapCache.HeightMaps[location];
			}
			else
			{
				noiseManager.Areas.Generators[NoiseAreaType.Mountains].Generate(x, z);
				surface = noiseManager.Areas.Generators[NoiseAreaType.Mountains].Output;

				heightMapCache.HeightMaps.Add(location, noiseManager.Areas.Generators[NoiseAreaType.Mountains].Output);
			}
		}
	}
}