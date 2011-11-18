using System;
using System.Linq;
using Framework.Core.Common;
using Game.World.Terrains.Generators.Noise.Managers;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Generators.Noise.Managers.World;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Tiles;

namespace Game.World.Terrains.Generators.Areas
{
	public class AreaGeneratorDefault : IAreaGenerator
	{
		private NoiseManager noiseManager;
		private byte[] surface;

		public AreaGeneratorDefault(int seed)
		{
			throw new NotImplementedException("Do not use this one");

			noiseManager = new NoiseManager(seed, Area.Size.X, Area.Size.Z, Area.Size.Y);

			surface = new byte[Area.LevelSize];
		}

		public Area Generate(int x, int y, int z)
		{
			// Initialize the area
			var area = new Area();
			area.Info.Location = new Vector3i(x, y, z);

			// Generate the area types surrounding this location
			noiseManager.World.Generators[NoiseWorldType.Default].Generate(x - 1, y - 1);
			byte[] areaTypes = noiseManager.World.Generators[NoiseWorldType.Default].Output;

			// Surface level terrain
			if (y >= 0)
			{
				NoiseAreaType areaTheme;

				if (x < 0)
				{
					areaTheme = NoiseAreaType.Plains;

					noiseManager.Areas.Generators[NoiseAreaType.Plains].Generate(x, z);
					surface = noiseManager.Areas.Generators[NoiseAreaType.Plains].Output;
				}
				else
				{
					areaTheme = NoiseAreaType.Mountains;

					noiseManager.Areas.Generators[NoiseAreaType.Mountains].Generate(x, z);
					surface = noiseManager.Areas.Generators[NoiseAreaType.Mountains].Output;
				}

				// Down/up (vertically)
				for (int tileY = 0; tileY < Area.Size.Y; tileY++)
				{
					int currentY = tileY * Area.LevelSize;
					int currentTileY = (Area.Size.Y - (tileY + 1)) + (y * Area.Size.Y);

					// In/out
					for (int tileZ = 0; tileZ < Area.Size.Z; tileZ++)
					{
						int currentZ = tileZ * Area.Size.Z;

						// Left/right (horizontally)
						for (int tileX = 0; tileX < Area.Size.X; tileX++)
						{
							int currentIndex = currentY + currentZ + tileX;
							int currentSurfaceIndex = currentZ + tileX;
							int currentSurfaceY = surface[currentSurfaceIndex];

							// Above the ground?
							if (currentTileY > currentSurfaceY)
							{
								// Empty tile
								area.Info.Tiles[currentIndex].Type = TileType.Empty;
							}
							// At the exact ground level?
							else if (currentTileY == currentSurfaceY)
							{
								if (areaTheme == NoiseAreaType.Plains)
								{
									area.Info.Tiles[currentIndex].Type = TileType.Grass;
								}
								else if (areaTheme == NoiseAreaType.Mountains)
								{
									area.Info.Tiles[currentIndex].Type = TileType.Sand;
								}
							}
							// Below the ground level
							else
							{
								area.Info.Tiles[currentIndex].Type = TileType.Sand;
							}
						}
					}
				}
			}
			// Underground level terrain
			else
			{
				for (int i = 0; i < area.Info.Tiles.Count(); i++)
				{
					area.Info.Tiles[i].Type = TileType.Sand;
				}
			}

			UpdateAreaInfo(ref area.Info);

			return area;
		}

		public void ReInitialize()
		{
		}

		private void UpdateAreaInfo(ref AreaInfo info)
		{
			// Calculate if the area is empty or not
			info.IsEmpty = info.Tiles.All(p => p.Type == TileType.Empty);
		}
	}
}