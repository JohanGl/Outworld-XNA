namespace Framework.Core.Common
{
	public struct Vector3b
	{
		public byte X;
		public byte Y;
		public byte Z;

		public Vector3b(byte x, byte y, byte z) : this()
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2}", X, Y, Z);
		}

		public static string ConvertToString(byte x, byte y, byte z)
		{
			return string.Format("{0},{1},{2}", x, y, z);
		}
	}
}