using Game.World.Terrains.Generators.Noise.Managers.AreaResources;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Generators.Noise.Managers.World;

namespace Game.World.Terrains.Generators.Noise.Managers
{
	public class NoiseManager
	{
		public WorldGenerator World;
		public AreaGenerator Areas;
		public AreaResourceGenerator AreaResources;

		/// <summary>
		/// Initializes the noise manager
		/// </summary>
		/// <param name="seed">The random seed to use</param>
		/// <param name="width">The width of the generated results</param>
		/// <param name="height">The height of the generated results</param>
		/// <param name="depth">The depth of the generated results</param>
		public NoiseManager(int seed, int width, int height, int depth)
		{
			// World theme generators (theme per area)
			World = new WorldGenerator(seed, 3, 3);

			// Area generators
			Areas = new AreaGenerator(seed, width, height);

			AreaResources = new AreaResourceGenerator(seed, width, height, depth);
			AreaResources.Generators[NoiseAreaResourceType.Default].SetOutputSize(width, height, depth);
		}
	}
}