using System.Collections.Generic;
using Game.World.Terrains.Generators.Noise.Helpers;

namespace Game.World.Terrains.Generators.Noise.Managers.Areas
{
	public class AreaGenerator
	{
		public Dictionary<NoiseAreaType, INoiseGenerator> Generators;
		public NoiseModifiers Modifiers;

		private int seed;
		private int width;
		private int height;

		public AreaGenerator(int seed, int width, int height)
		{
			this.seed = seed;
			this.width = width;
			this.height = height;

			// Initialize the terrain-type generators
			Generators = new Dictionary<NoiseAreaType, INoiseGenerator>();
			Generators[NoiseAreaType.Plains] = InitializePlains();
			Generators[NoiseAreaType.Mountains] = InitializeMountains();
			Generators[NoiseAreaType.Mountains_Ground] = InitializeMountainsGround();
			Generators[NoiseAreaType.Custom] = InitializeCustom();

			// Initialize the modifiers handler
			Modifiers = new NoiseModifiers();
		}

		#region Generator initializers

		private INoiseGenerator InitializePlains()
		{
			var generator = new NoiseGeneratorDefault();

			var settings = new NoiseGeneratorSettings
						   {
							   Seed = seed,
							   Frequency = 0.04,
							   Persistence = 0.3,
							   Octaves = 8,
							   Amplitude = 0.01,
							   CloudCoverage = 0.17,
							   CloudDensity = 1
						   };

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		private INoiseGenerator InitializeMountains()
		{
			var generator = new NoiseGeneratorDefault();

			var settings = new NoiseGeneratorSettings
						   {
							   Seed = seed,
							   Frequency = 0.006,
							   Persistence = 0.65,
							   Octaves = 8,
							   Amplitude = 0.45,
							   CloudCoverage = 0,
							   CloudDensity = 1
						   };

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		private INoiseGenerator InitializeMountainsGround()
		{
			var generator = new NoiseGeneratorDefault();

			var settings = new NoiseGeneratorSettings
						   {
							   Seed = seed,
							   Frequency = 0.07,
							   Persistence = 0.8,
							   Octaves = 8,
							   Amplitude = 0.01,
							   CloudCoverage = 0.02,
							   CloudDensity = 0.5
						   };

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		private INoiseGenerator InitializeCustom()
		{
			var generator = new NoiseGeneratorDefault();

			var settings = new NoiseGeneratorSettings
						   {
							   Seed = seed,
							   Frequency = 0,
							   Persistence = 0,
							   Octaves = 0,
							   Amplitude = 0,
							   CloudCoverage = 0,
							   CloudDensity = 0
						   };

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		#endregion
	}
}