using System.Collections.Generic;
using Framework.Core.Contexts;
using Game.World.Terrains.Parts.Areas.AreaModels.Builders;
using Game.World.Terrains.Visibility;

namespace Game.World.Terrains.Parts.Areas.AreaModels
{
	public static class AreaModelHandler
	{
		private static AreaModelBuilder Builder;

		public static void Initialize(TerrainVisibility terrainVisibility, GameContext context)
		{
			Builder = new AreaModelBuilder(terrainVisibility);
		}

		public static void Build(Area area)
		{
			Builder.Build(area);
		}

		public static void Build(Area area, List<Area> neighbors)
		{
			Builder.Build(area, neighbors);
		}
	}
}