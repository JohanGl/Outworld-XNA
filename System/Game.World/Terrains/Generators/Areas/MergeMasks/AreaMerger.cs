using System;
using System.Collections.Generic;
using Framework.Core.Common;
using Game.World.Terrains.Generators.Areas.Tiles;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Parts.Areas;

namespace Game.World.Terrains.Generators.Areas.MergeMasks
{
	public class AreaMerger
	{
		private Dictionary<NoiseAreaType, IAreaTilesGenerator> areaTilesGenerators;
		private Random random;
		private readonly int AreaEdgeThreshold;

		public AreaMerger(Dictionary<NoiseAreaType, IAreaTilesGenerator> areaTilesGenerators)
		{
			this.areaTilesGenerators = areaTilesGenerators;
			AreaEdgeThreshold = 235;
		}

		public void Merge(ref Area area, Area areaMask, byte[] mask)
		{
			if (area.Info.IsEmpty)
			{
				HandleEmptyArea(area, areaMask, mask);
			}
			else
			{
				HandleArea(area, areaMask, mask);
			}
		}

		private void HandleEmptyArea(Area area, Area areaMask, byte[] mask)
		{
			var location2i = new Vector2i(area.Info.Location.X, area.Info.Location.Z);

			bool isEmpty = true;

			// Loop through all columns of the area
			for (int z = 0; z < Area.Size.Z; z++)
			{
				int currentZ = z * Area.Size.X;

				for (int x = 0; x < Area.Size.X; x++)
				{
					// Skip unaffected columns
					if (mask[currentZ + x] == 0)
					{
						continue;
					}

					// Get the heights of the two areas at the current column location
					int areaHeight = areaTilesGenerators[area.Info.Type].GetHeight(location2i, x, z);
					int maskAreaHeight = areaTilesGenerators[areaMask.Info.Type].GetHeight(location2i, x, z);

					// Calculate the new height based on the weight between the two areas
					float maskPercentage = mask[currentZ + x] / 255f;
					int delta = Math.Max(0, (int)((maskAreaHeight - areaHeight) * maskPercentage));
					int newHeight = areaHeight + delta;

					// Completely filled column
					if (newHeight > area.Info.TileHeight.End)
					{
						isEmpty = false;
						areaTilesGenerators[area.Info.Type].ExpandHeight(ref area, x, z, newHeight);
					}
					// The column height is within the bounds of the area
					else if (delta > 0)
					{
						newHeight -= area.Info.TileHeight.Start;
						areaTilesGenerators[area.Info.Type].ExpandHeight(ref area, x, z, newHeight);

						if (isEmpty)
						{
							isEmpty = newHeight > 0;
						}
					}
				}
			}

			area.Info.IsEmpty = isEmpty;
		}

		private void HandleArea(Area area, Area areaMask, byte[] mask)
		{
			var location2i = new Vector2i(area.Info.Location.X, area.Info.Location.Z);

			random = new Random(location2i.GetHashCode());
			NoiseAreaType noiseAreaType;

			// TODO: Remove this debug code
			if (area.Info.Location.ToString() == "0,0,3")
			{
				int a = 10;
			}

			// Loop through all columns of the area
			for (int z = 0; z < Area.Size.Z; z++)
			{
				int currentZ = z * Area.Size.X;

				for (int x = 0; x < Area.Size.X; x++)
				{
					// Skip unaffected columns
					if (mask[currentZ + x] == 0)
					{
						continue;
					}

					// Get the heights of the two areas at the current column location
					int areaHeight = areaTilesGenerators[area.Info.Type].GetHeight(location2i, x, z);
					int maskAreaHeight = areaTilesGenerators[areaMask.Info.Type].GetHeight(location2i, x, z);
					int newAreaHeight;

					// For the outer edges of the mask affected by this threshold, clamp the area height to the areaMask height for a rounded/smoother visual effect
					if (mask[currentZ + x] > AreaEdgeThreshold)
					{
						newAreaHeight = areaTilesGenerators[areaMask.Info.Type].GetHeight(location2i, x, z);
						noiseAreaType = areaMask.Info.Type;
					}
					else
					{
						float maskPercentage = mask[currentZ + x] / (float)AreaEdgeThreshold;

						newAreaHeight = Math.Max(0, areaHeight + (int)((maskAreaHeight - areaHeight) * maskPercentage));

						// Find out if we should use the theme from the area or the areaMask for this tile column (weighted for a gradient effect by using the mask[])
						noiseAreaType = random.Next(256) < mask[currentZ + x] ? areaMask.Info.Type : area.Info.Type;
					}

					// Lower the column
					if (maskAreaHeight < areaHeight)
					{
						newAreaHeight = Math.Max(0, newAreaHeight - area.Info.TileHeight.Start);
						areaTilesGenerators[noiseAreaType].TrimHeight(ref area, x, z, newAreaHeight);
					}
					// Raise the column
					else if (maskAreaHeight > areaHeight)
					{
						newAreaHeight = Math.Min(Area.Size.Y - 1, newAreaHeight - areaMask.Info.TileHeight.Start);
						areaTilesGenerators[noiseAreaType].ExpandHeight(ref area, x, z, newAreaHeight);
					}
				}
			}
		}
	}
}