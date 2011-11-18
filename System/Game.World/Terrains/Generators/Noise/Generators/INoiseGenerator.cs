namespace Game.World.Terrains.Generators.Noise
{
	/// <summary>
	/// Base interface for all noise generator implementations
	/// </summary>
	public interface INoiseGenerator
	{
		byte[] Output { get; set; }

		void Initialize(NoiseGeneratorSettings settings);

		void SetOutputSize(int width, int height, int depth = 1);

		void Generate(int x, int y);
		void Generate(int x, int y, int z);

		byte GenerateSinglePoint(int x, int y);
		byte GenerateSinglePoint(int x, int y, int z);
	}
}