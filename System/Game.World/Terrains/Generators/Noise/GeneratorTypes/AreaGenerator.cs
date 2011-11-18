using System.Collections.Generic;

namespace Game.World.Terrains.Generators.Noise
{
	public class AreaGenerator
	{
		public Dictionary<NoiseAreaType, INoiseGenerator> AreaGenerators;
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
			AreaGenerators = new Dictionary<NoiseAreaType, INoiseGenerator>();
			AreaGenerators[NoiseAreaType.Plains] = InitializePlains();
			AreaGenerators[NoiseAreaType.Mountains] = InitializeMountains();
			AreaGenerators[NoiseAreaType.Mountains_Ground] = InitializeMountainsBase();
			AreaGenerators[NoiseAreaType.Custom] = InitializeCustom();

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

		private INoiseGenerator InitializeMountains()
		{
			var generator = new NoiseGeneratorDefault();

			var settings = new NoiseGeneratorSettings
			               {
			               	Seed = seed,
			               	Frequency = 0.006,
			               	Persistence = 0.65,
			               	Octaves = 8,
			               	Amplitude = 0.065,
			               	CloudCoverage = 0,
			               	CloudDensity = 1
			               };

			generator.Initialize(settings);
			generator.SetOutputSize(width, height);

			return generator;
		}

		private INoiseGenerator InitializeMountainsBase()
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

		//private INoiseGenerator InitializeTerrainSpawners()
		//{
		//    var generator = new NoiseGeneratorDefault();

		//    var settings = new NoiseGeneratorSettings
		//    {
		//        Seed = seed,
		//        Frequency = 0.01,
		//        Persistence = 0.85,
		//        Octaves = 8,
		//        Amplitude = 0.5,
		//        CloudCoverage = 1.25,
		//        CloudDensity = 0.36
		//    };

		//    generator.Initialize(settings);
		//    generator.SetOutputSize(width, height);

		//    return generator;
		//}

		#endregion
	}
}