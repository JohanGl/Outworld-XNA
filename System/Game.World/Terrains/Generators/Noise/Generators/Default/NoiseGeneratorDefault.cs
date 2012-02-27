using System;
using Framework.Core.Common;

namespace Game.World.Terrains.Generators.Noise
{
	public class NoiseGeneratorDefault : INoiseGenerator
	{
		public byte[] Output { get; set; }

		#region private members

		private NoiseGeneratorSettings settings;
		private Random random;
		private int random1;
		private int random2;
		private int random3;
		private int width;
		private int height;
		private int depth;
		private int offset;

		#endregion

		public NoiseGeneratorDefault()
		{
			SetOutputSize(1, 1);
		}

		public void Initialize(NoiseGeneratorSettings settings)
		{
			this.settings = settings;

			random = new Random(settings.Seed);
			random1 = random.Next(1000, 10000);
			random2 = random.Next(100000, 1000000);
			random3 = random.Next(1000000000, 2000000000);
			offset = Int32.MaxValue / 2;
		}

		public void SetOutputSize(int width, int height, int depth = 1)
		{
			this.width = width;
			this.height = height;
			this.depth = depth;

			Output = new byte[width * height * depth];
		}

		public Vector3i GetOutputSize()
		{
			return new Vector3i(width, height, depth);
		}

		public void Generate(int x, int y)
		{
			int index = 0;

			for (int currentY = y * height; currentY < (y * height) + height; currentY++)
			{
				for (int currentX = (x * width); currentX < (x * width) + width; currentX++)
				{
					// Calculate the current pixel and add it to the result pixel array
					Output[index++] = (byte)(GeneratePixel(offset + currentX, offset + currentY) * 255);
				}
			}
		}

		public void Generate(int x, int y, int z)
		{
			Generate(x, y);
		}

		public byte GenerateSinglePoint(int x, int y)
		{
			return (byte)(GeneratePixel(offset + (x * width), offset + (y * height)) * 255);
		}

		public byte GenerateSinglePoint(int x, int y, int z)
		{
			throw new NotSupportedException("Not supported yet");
		}

		#region private

		/// <summary>
		/// Calculates the perlin noise for a specific coordinate
		/// </summary>
		/// <param name="x">The x coordinate</param>
		/// <param name="y">The y coordinate</param>
		/// <returns>The perlin noise value in the interval 0.0 to 1.0</returns>
		private double GeneratePixel(int x, int y)
		{
			double total = 0.0;
			double frequency = settings.Frequency;
			double amplitude = settings.Amplitude;

			for (int i = 0; i < settings.Octaves; i++)
			{
				total = total + Smooth(x * frequency, y * frequency) * amplitude;
				frequency = frequency * 2;
				amplitude = amplitude * settings.Persistence;
			}

			total = (total + settings.CloudCoverage) * settings.CloudDensity;

			if (total < 0) total = 0.0;
			if (total > 1) total = 1.0;

			return total;
		}

		/// <summary>
		/// Smoothens a perlin noise pixel
		/// </summary>
		/// <param name="x">The x coordinate</param>
		/// <param name="y">The y coordinate</param>
		/// <returns></returns>
		private double Smooth(double x, double y)
		{
			double n1 = Noise((int)x, (int)y);
			double n2 = Noise((int)x + 1, (int)y);
			double n3 = Noise((int)x, (int)y + 1);
			double n4 = Noise((int)x + 1, (int)y + 1);

			double i1 = Interpolate(n1, n2, x - (int)x);
			double i2 = Interpolate(n3, n4, x - (int)x);

			return Interpolate(i1, i2, y - (int)y);
		}

		/// <summary>
		/// Gets perlin noise for the specified coordinates
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns></returns>
		private double Noise(int x, int y)
		{
			int n = x + y * 57;
			n = (n << 13) ^ n;

			return (1.0 - ((n * (n * n * random1 + random2) + random3) & 0x7fffffff) / 1073741824.0);
		}

		/// <summary>
		/// Interpolates the specified coordinate
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="a"></param>
		/// <returns></returns>
		private double Interpolate(double x, double y, double a)
		{
			double val = (1 - Math.Cos(a * Math.PI)) * .5;
			return x * (1 - val) + y * val;
		}

		#endregion
	}
}