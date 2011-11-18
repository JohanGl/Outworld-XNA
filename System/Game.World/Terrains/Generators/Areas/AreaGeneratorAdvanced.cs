using System.Collections.Generic;
using Framework.Core.Common;
using Game.World.Terrains.Generators.Areas.MergeMasks;
using Game.World.Terrains.Generators.Areas.Themes;
using Game.World.Terrains.Generators.Areas.Tiles;
using Game.World.Terrains.Generators.Noise.Managers;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Helpers;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Tiles;

namespace Game.World.Terrains.Generators.Areas
{
	public class AreaGeneratorAdvanced : IAreaGenerator
	{
		private static AreaMergeMaskImageProvider mergeMaskImageProvider;
		private AreaThemeHandler themeHandler;
		private AreaMerger areaMerger;
		private NoiseManager noiseManager;
		private Dictionary<NoiseAreaType, IAreaTilesGenerator> areaTilesGenerators;

		public AreaGeneratorAdvanced(int seed)
		{
			noiseManager = new NoiseManager(seed, Area.Size.X, Area.Size.Z, Area.Size.Y);
			themeHandler = new AreaThemeHandler(noiseManager);

			InitializeAreaTilesGenerators();
			InitializeMergeMask();

			areaMerger = new AreaMerger(areaTilesGenerators);
		}

		private void InitializeAreaTilesGenerators()
		{
			areaTilesGenerators = new Dictionary<NoiseAreaType, IAreaTilesGenerator>();

			areaTilesGenerators.Add(NoiseAreaType.Plains, new PlainsGenerator(noiseManager));
			areaTilesGenerators.Add(NoiseAreaType.Mountains, new MountainsGenerator(noiseManager));
		}

		private void InitializeMergeMask()
		{
			if (mergeMaskImageProvider == null)
			{
				mergeMaskImageProvider = new AreaMergeMaskImageProvider();
			}
		}

		public Area Generate(int x, int y, int z)
		{
			// Generate the theme of the area and its 8 surrounding neighbors
			themeHandler.GenerateAreaTheme(x, y, z);

			// Generate the area
			var area = Generate(themeHandler.CenterAreaType, x, y, z);

			// Post-process the area if it contains any tiles
			MergeAreaWithNeighbors(ref area);

			return area;
		}

		public void ReInitialize()
		{
			foreach (var pair in areaTilesGenerators)
			{
				pair.Value.ClearCache();
			}
		}

		private Area Generate(NoiseAreaType type, int x, int y, int z)
		{
			// Initialize the area
			var area = new Area();
			area.Info.Location = new Vector3i(x, y, z);

			// The area is below the surface
			if (y < 0)
			{
				areaTilesGenerators[type].GenerateUnderground(ref area);
			}
			// The area is at the surface or above
			else
			{
				areaTilesGenerators[type].GenerateSurface(ref area);
			}

			// Initializes the common settings
			SetAreaInfo(ref area.Info, type);

			return area;
		}

		private void SetAreaInfo(ref AreaInfo info, NoiseAreaType type)
		{
			// Set the area type
			info.Type = type;

			// Set the flag indicating whether the area is empty or not determined by checking if all tiletypes are empty
			info.IsEmpty = true;

			for (int i = 0; i < Area.TotalTiles; i++)
			{
				if (info.Tiles[i].Type != TileType.Empty)
				{
					info.IsEmpty = false;
					break;
				}
			}
		}

		private void MergeAreaWithNeighbors(ref Area area)
		{
			// Skip this step if we have nothing to merge or if we are underground
			if (area.Info.Location.Y < 0 ||
				themeHandler.IsAllAreasOfTheSameType())
			{
				return;
			}

			// Get all distinct area types ordered by lowest to highest that have a higher value than the center area type
			var types = themeHandler.GetSortedAndDistinctNoiseAreaTypes(true);

			// Loop through all higher types and process their masks
			for (int i = 0; i < types.Count; i++)
			{
				var currentType = (NoiseAreaType)types[i];

				// Get the mask for the current type
				var mask = mergeMaskImageProvider.GetMergeMask(themeHandler.AreaTypes, currentType);

				// Generate the mask area representing the current type to merge with the area
				var maskArea = Generate(currentType, area.Info.Location.X, area.Info.Location.Y, area.Info.Location.Z);

				// Perform the merging of the area and its current mask area
				areaMerger.Merge(ref area, maskArea, mask);
			}
		}
	}
}