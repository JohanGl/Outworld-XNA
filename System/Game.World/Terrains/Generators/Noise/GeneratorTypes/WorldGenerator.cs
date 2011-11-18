using System.Collections.Generic;

namespace Game.World.Terrains.Generators.Noise
{
	public class WorldGenerator
	{
		public Dictionary<NoiseWorldType, INoiseGenerator> Generators;
		public NoiseModifiers Modifiers;

		private int seed;
		private int width;
		private int height;

		public WorldGenerator(int seed, int width, int height)
		{
			this.seed = seed;
			this.width = width;
			this.height = height;

			// Initialize the terrain-type generators
			Generators = new Dictionary<NoiseWorldType, INoiseGenerator>();
			Generators[NoiseWorldType.Default] = InitializeDefault();

			// Initialize the modifiers handler
			Modifiers = new NoiseModifiers();
		}

		#region Generator initializers

		private INoiseGenerator InitializeDefault()
		{
			var generator = new NoiseGeneratorDefault();

			var settings = new NoiseGeneratorSettings
			               {
			               	Seed = seed,
			               	Frequency = 0.01,
			               	Persistence = 0.65,
			               	Octaves = 8,
			               	Amplitude = 0.03,
			               	CloudCoverage = 0.5,
			               	CloudDensity = 0.8
			               };

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		#endregion
	}
}