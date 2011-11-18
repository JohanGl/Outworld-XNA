namespace Game.World.Terrains.Generators.Noise.Helpers
{
	public enum CombineMode
	{
		Average,
		Add,
		Subtract,
		Multiply
	}

	/// <summary>
	/// Handles modifications on generated noise
	/// </summary>
	public class NoiseModifiers
	{
		/// <summary>
		/// Combines two noise sections with eachother using the average value
		/// </summary>
		/// <param name="a">First array which also becomes the result of the operation</param>
		/// <param name="b">Second array</param>
		/// <param name="mode">The type of combination operation to perform</param>
		public void Combine(ref byte[] a, byte[] b, CombineMode mode)
		{
			switch (mode)
			{
				case CombineMode.Average:
					{
						// Loop through all values of the arrays
						for (int i = 0; i < a.Length; i++)
						{
							// Calculate the average value of the two arrays and put the result in the first array
							a[i] = (byte)((a[i] + b[i]) * 0.5d);
						}
					}
					break;

				case CombineMode.Add:
					{
						// Loop through all values of the arrays
						for (int i = 0; i < a.Length; i++)
						{
							// Addition
							int result = a[i] + b[i];

							if (result > 255)
							{
								result = 255;
							}

							a[i] = (byte)result;
						}
					}
					break;

				case CombineMode.Subtract:
					{
						// Loop through all values of the arrays
						for (int i = 0; i < a.Length; i++)
						{
							// Subtraction
							int result = a[i] - b[i];

							if (result < 0)
							{
								result = 0;
							}

							a[i] = (byte)result;
						}
					}
					break;

				case CombineMode.Multiply:
					{
						// Loop through all values of the arrays
						for (int i = 0; i < a.Length; i++)
						{
							// Subtraction
							int result = a[i] * b[i];

							if (result > 255)
							{
								result = 255;
							}

							a[i] = (byte)result;
						}
					}
					break;
			}
		}

		//int result = a[i] + b[i];

		//if (result > 255)
		//{
		//    result = 255;
		//}

		//a[i] = (byte)result;

	}
}