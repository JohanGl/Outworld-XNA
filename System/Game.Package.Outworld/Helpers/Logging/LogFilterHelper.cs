using Framework.Core.Diagnostics.Logging;
using Game.World.Terrains.Generators;
using Game.World.Terrains.Rendering;
using Game.World.Terrains.Visibility;
using Game.World.Terrains.Visibility.Queues;

namespace Outworld.Helpers.Logging
{
	public class LogFilterHelper
	{
		public void FilterTerrain()
		{
			Logger.Mute<TerrainGenerator>(true);
			Logger.Mute<TerrainRenderer>(true);
			Logger.Mute<TerrainVisibility>(true);
			Logger.Mute<VisibilityQueue>(true);
		}
	}
}