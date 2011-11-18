using Game.World.Terrains.Parts.Areas;

namespace Game.World.Terrains.Generators.Areas
{
	public interface IAreaGenerator
	{
		Area Generate(int x, int y, int z);
		void ReInitialize();
	}
}