using System;

namespace Game.World.Terrains.Generators.Noise
{
	public class NoiseGeneratorLibNoise : INoiseGenerator
	{
		public byte[] Output { get; set; }

		#region private members

		private NoiseGeneratorSettings settings;
		private Random random;
		private int width;
		private int height;
		private int depth;
		private int offset;
		private double lacunarity;

		#endregion

		public NoiseGeneratorLibNoise()
		{
			SetOutputSize(1, 1);
		}

		public void Initialize(NoiseGeneratorSettings settings)
		{
			this.settings = settings;

			random = new Random(settings.Seed);
			offset = Int32.MaxValue / 2;

			lacunarity = 2.0f;
			Utils.m_quality = Utils.QualityMode.Low;
		}

		public void SetOutputSize(int width, int height, int depth = 1)
		{
			this.width = width;
			this.height = height;
			this.depth = depth;

			Output = new byte[width * height * depth];
		}

		public void Generate(int x, int y)
		{
			int index = 0;

			for (int currentY = y * height; currentY < (y * height) + height; currentY++)
			{
				for (int currentX = (x * width); currentX < (x * width) + width; currentX++)
				{
					// Calculate the current pixel and add it to the result pixel array
					Output[index++] = (byte)(GenerateSinglePoint(offset + currentX, offset + currentY) * 255);
				}
			}
		}

		public void Generate(int x, int y, int z)
		{
			int index = 0;

			// Down/up (vertically)
			for (int currentY = y * height; currentY < (y * height) + height; currentY++)
			{
				// In/out
				for (int currentZ = z * depth; currentZ < (z * depth) + depth; currentZ++)
				{
					// Left/right (horizontally)
					for (int currentX = (x * width); currentX < (x * width) + width; currentX++)
					{
						Output[index++] = GenerateSinglePoint(offset + currentX, offset + currentY, offset + currentZ);
					}
				}
			}
		}

		public byte GenerateSinglePoint(int x, int y)
		{
			throw new NotImplementedException();
		}

		public byte GenerateSinglePoint(int x, int y, int z)
		{
			double value = 0.0;
			double signal = 0.0;
			double cp = 1.0;
			double nx, ny, nz;
			long seed;

			double dx = x * settings.Frequency;
			double dy = y * settings.Frequency;
			double dz = z * settings.Frequency;

			for (int i = 0; i < settings.Octaves; i++)
			{
				nx = Utils.MakeInt32Range(dx);
				ny = Utils.MakeInt32Range(dy);
				nz = Utils.MakeInt32Range(dz);
				seed = (settings.Seed + i) & 0xffffffff;
				signal = Utils.GradientCoherentNoise3D(nx, ny, nz, seed, Utils.m_quality);
				value += signal * cp;
				dx *= lacunarity;
				dy *= lacunarity;
				dz *= lacunarity;
				cp *= settings.Persistence;
			}

			return (byte)(value * 255);
		}
	}
}