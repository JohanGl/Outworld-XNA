using System.Collections.Generic;

namespace Game.World.Terrains.Generators.Noise
{
	public class NoiseManager
	{
		public Dictionary<NoiseGeneratorType, NoiseGenerator> Generators;
		public NoiseModifiers Modifiers;

		private int? seed;

		/// <summary>
		/// Initializes the noise manager
		/// </summary>
		/// <param name="seed">The random seed to use</param>
		/// <param name="width">The width of the generated results</param>
		/// <param name="height">The height of the generated results</param>
		public NoiseManager(int? seed, int width, int height)
		{
			// Set the terrain seed
			this.seed = seed;

			// Initialize the terrain-type generators
			Generators = new Dictionary<NoiseGeneratorType, NoiseGenerator>();
			Generators[NoiseGeneratorType.Plains] = InitializePlains(width, height);
			Generators[NoiseGeneratorType.Mountains] = InitializeMountains(width, height);
			Generators[NoiseGeneratorType.MountainsBase] = InitializeMountainsBase(width, height);
			Generators[NoiseGeneratorType.Lakes] = InitializeLakes(width, height);
			Generators[NoiseGeneratorType.Seas] = InitializeSeas(width, height);
			Generators[NoiseGeneratorType.TerrainSpawners] = InitializeTerrainSpawners(width, height);
			Generators[NoiseGeneratorType.Custom] = InitializeCustom(width, height);

			// Initialize the modifiers handler
			Modifiers = new NoiseModifiers();
		}

		#region Generator initializers

		private NoiseGenerator InitializePlains(int width, int height)
		{
			var generator = new NoiseGenerator(seed, width, height);

			generator.Settings.Frequency = 0.01;
			generator.Settings.Persistence = 0.65;
			generator.Settings.Octaves = 8;
			generator.Settings.Amplitude = 0.03;
			generator.Settings.CloudCoverage = 0.5;
			generator.Settings.CloudDensity = 0.8;

			return generator;
		}

		private NoiseGenerator InitializeMountains(int width, int height)
		{
			var generator = new NoiseGenerator(seed, width, height);

			generator.Settings.Frequency = 0.006;
			generator.Settings.Persistence = 0.65;
			generator.Settings.Octaves = 8;
			generator.Settings.Amplitude = 0.65;
			generator.Settings.CloudCoverage = 0;
			generator.Settings.CloudDensity = 1;

			return generator;
		}

		private NoiseGenerator InitializeMountainsBase(int width, int height)
		{
			var generator = new NoiseGenerator(seed, width, height);

			generator.Settings.Frequency = 0.07;
			generator.Settings.Persistence = 0.8;
			generator.Settings.Octaves = 8;
			generator.Settings.Amplitude = 0.01;
			generator.Settings.CloudCoverage = 0.02;
			generator.Settings.CloudDensity = 0.5;

			return generator;
		}

		private NoiseGenerator InitializeLakes(int width, int height)
		{
			var generator = new NoiseGenerator(seed, width, height);

			generator.Settings.Frequency = 0.006;
			generator.Settings.Persistence = 0.33;
			generator.Settings.Octaves = 8;
			generator.Settings.Amplitude = 0.1;
			generator.Settings.CloudCoverage = 0.75;
			generator.Settings.CloudDensity = 1;

			return generator;
		}

		private NoiseGenerator InitializeSeas(int width, int height)
		{
			var generator = new NoiseGenerator(seed, width, height);

			generator.Settings.Frequency = 0.006;
			generator.Settings.Persistence = 0.65;
			generator.Settings.Octaves = 8;
			generator.Settings.Amplitude = 0.1;
			generator.Settings.CloudCoverage = 0;
			generator.Settings.CloudDensity = 1;

			return generator;
		}

		private NoiseGenerator InitializeCustom(int width, int height)
		{
			var generator = new NoiseGenerator(seed, width, height);

			generator.Settings.Frequency = 0;
			generator.Settings.Persistence = 0;
			generator.Settings.Octaves = 0;
			generator.Settings.Amplitude = 0;
			generator.Settings.CloudCoverage = 0;
			generator.Settings.CloudDensity = 0;

			return generator;
		}

		private NoiseGenerator InitializeTerrainSpawners(int width, int height)
		{
			var generator = new NoiseGenerator(seed, width, height);

			generator.Settings.Frequency = 0.01;
			generator.Settings.Persistence = 0.85;
			generator.Settings.Octaves = 8;
			generator.Settings.Amplitude = 0.5;
			generator.Settings.CloudCoverage = 1.25;
			generator.Settings.CloudDensity = 0.36;

			//generator.Settings.Frequency = 0.008;
			//generator.Settings.Persistence = 1;
			//generator.Settings.Octaves = 8;
			//generator.Settings.Amplitude = 1;
			//generator.Settings.CloudCoverage = 3;
			//generator.Settings.CloudDensity = 0.125;

			return generator;
		}

		#endregion
	}
}