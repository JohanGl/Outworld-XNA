using Framework.Core.Diagnostics.Logging;
using Game.Network.Clients;
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

		public void FilterEverythingExceptGameClient()
		{
			Logger.Mute(true);
			Logger.Mute<GameClient>(false);
		}

		public void FilterAll()
		{
			Logger.Mute(true);
		}
	}
}