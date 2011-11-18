using System;

namespace Framework.Core.Common
{
	public struct Vector3i
	{
		public int X;
		public int Y;
		public int Z;

		public Vector3i(int xyz) : this()
		{
			X = xyz;
			Y = xyz;
			Z = xyz;
		}

		public Vector3i(int x, int y, int z) : this()
		{
			X = x;
			Y = y;
			Z = z;
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2}", X, Y, Z);
		}

		public static string ConvertToString(int x, int y, int z)
		{
			return string.Format("{0},{1},{2}", x, y, z);
		}

		public static double Distance(Vector3i a, Vector3i b)
		{
			a.X = b.X - a.X;
			a.Y = b.Y - a.Y;
			a.Z = b.Z - a.Z;

			return Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
		}
	}
}