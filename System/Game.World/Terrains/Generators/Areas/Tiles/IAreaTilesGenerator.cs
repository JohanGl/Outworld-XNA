using Framework.Core.Common;
using Game.World.Terrains.Parts.Areas;

namespace Game.World.Terrains.Generators.Areas.Tiles
{
	public interface IAreaTilesGenerator
	{
		void GenerateSurface(ref Area area);
		void GenerateUnderground(ref Area area);
		int GetHeight(Vector2i location, int x, int z);
		void TrimHeight(ref Area area, int x, int z, int height);
		void ExpandHeight(ref Area area, int x, int z, int height);
		void ClearCache();
	}
}