using Game.World.Terrains.Generators;
using Game.World.Terrains.Generators.Areas;

namespace Game.Network.Servers.Simulations.World.Terrains
{
	public class TerrainContext
	{
		public TerrainGenerator Generator;

		public TerrainContext(int seed)
		{
			Generator = new TerrainGenerator(new AreaGeneratorAdvanced(seed));
		}
	}
}