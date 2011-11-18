using System.Collections.Generic;
using Framework.Core.Common;
using Game.World.Terrains.Generators.Noise.Managers;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Generators.Noise.Managers.World;
using Game.World.Terrains.Helpers;

namespace Game.World.Terrains.Generators.Areas.Themes
{
	public class AreaThemeHandler
	{
		public NoiseAreaType[] AreaTypes { get; private set; }
	
		public NoiseAreaType CenterAreaType
		{
			get
			{
				return AreaTypes[4];
			}
		}

		private NoiseManager noiseManager;

		public AreaThemeHandler(NoiseManager noiseManager)
		{
			this.noiseManager = noiseManager;

			AreaTypes = new NoiseAreaType[9];
		}

		public void GenerateAreaTheme(int x, int y, int z)
		{
			int index = 0;

			// Generate the area types surrounding this location (including itself) in a 3x3 range
			for (int currentZ = z - 1; currentZ <= z + 1; currentZ++)
			{
				for (int currentX = x - 1; currentX <= x + 1; currentX++)
				{
					byte point = noiseManager.World.Generators[NoiseWorldType.Default].GenerateSinglePoint(currentX, currentZ);

					AreaTypes[index++] = ConvertPointToAreaType(point);
				}
			}
		}

		private NoiseAreaType ConvertPointToAreaType(byte point)
		{
			if (point < 128)
			{
				return NoiseAreaType.Plains;
			}
			else
			{
				return NoiseAreaType.Mountains;
			}
		}

		public List<int> GetSortedAndDistinctNoiseAreaTypes(bool onlyTakeTypesHigherThanCenterType)
		{
			var sortedList = new List<int>();

			for (int i = 0; i < AreaTypes.Length; i++)
			{
				var currentType = (int)AreaTypes[i];

				// Always add the first entry
				if (sortedList.Count == 0)
				{
					// Skip types lower than the center type?
					if (onlyTakeTypesHigherThanCenterType)
					{
						if (currentType <= (int)CenterAreaType)
						{
							continue;
						}
					}

					sortedList.Add(currentType);
				}
				// Locate where to put the current type
				else
				{
					// Loop through all existing types
					for (int j = 0; j < sortedList.Count; j++)
					{
						// Skip types lower than the center type?
						if (onlyTakeTypesHigherThanCenterType)
						{
							if (currentType <= (int)CenterAreaType)
							{
								break;
							}
						}

						// If the current type already exists we exit without adding it
						if (currentType == sortedList[j])
						{
							break;
						}

						// If the current type is lower than the result index then insert it here
						if (currentType < sortedList[j])
						{
							sortedList.Insert(j, currentType);
							break;
						}

						// If the current type is at the last result index then always add it
						if (j == sortedList.Count - 1)
						{
							sortedList.Add(currentType);
							break;
						}
					}
				}
			}

			return sortedList;
		}

		public bool IsAllAreasOfTheSameType()
		{
			for (int i = 1; i < AreaTypes.Length; i++)
			{
				if (AreaTypes[i] != AreaTypes[0])
				{
					return false;
				}
			}

			return true;
		}
	}
}