using System.Collections.Generic;

namespace Game.World.Terrains.Generators.Noise.Managers.AreaResources
{
	public class AreaResourceGenerator
	{
		public Dictionary<NoiseAreaResourceType, INoiseGenerator> Generators;

		private int seed;
		private int width;
		private int height;
		private int depth;

		public AreaResourceGenerator(int seed, int width, int height, int depth)
		{
			this.seed = seed;
			this.width = width;
			this.height = height;
			this.depth = depth;

			// Initialize the arearesource-type generators
			Generators = new Dictionary<NoiseAreaResourceType, INoiseGenerator>();
			Generators[NoiseAreaResourceType.Default] = InitializeDefault();
		}

		#region Generator initializers

		private INoiseGenerator InitializeDefault()
		{
			var generator = new NoiseGeneratorLibNoise();

			var settings = new NoiseGeneratorSettings
			{
				Seed = seed,
				Frequency = 0.04,
				Persistence = 0.3,
				Octaves = 1,
				Amplitude = 0.01,
				CloudCoverage = 0.17,
				CloudDensity = 1
			};

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		#endregion
	}
}