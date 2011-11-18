using System.Collections.Generic;
using Game.World.Terrains.Generators.Noise.Helpers;

namespace Game.World.Terrains.Generators.Noise.Managers.World
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
			Generators[NoiseWorldType.Decorator] = InitializeDecorator();

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
							   Frequency = 0.008,
							   Persistence = 0.9,
							   Octaves = 8,
							   Amplitude = 2,
							   CloudCoverage = 6,
							   CloudDensity = 0.1
						   };

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		private INoiseGenerator InitializeDecorator()
		{
			var generator = new NoiseGeneratorDefault();

			var settings = new NoiseGeneratorSettings
			{
				Seed = seed,
				Frequency = 0.007,
				Persistence = 1,
				Octaves = 8,
				Amplitude = 1,
				CloudCoverage = 0,
				CloudDensity = 0.125
			};

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		#endregion
	}
}