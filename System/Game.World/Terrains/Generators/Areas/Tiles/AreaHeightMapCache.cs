using System.Collections.Generic;
using Framework.Core.Common;

namespace Game.World.Terrains.Generators.Areas.Tiles
{
	public class AreaHeightMapCache
	{
		public Dictionary<Vector2i, byte[]> HeightMaps;

		public AreaHeightMapCache()
		{
			HeightMaps = new Dictionary<Vector2i, byte[]>();
		}
	}
}