namespace Framework.Core.Common
{
	public struct Vector2i
	{
		public int X;
		public int Y;

		public Vector2i(int x, int y) : this()
		{
			this.X = x;
			this.Y = y;
		}

		public override string ToString()
		{
			return string.Format("{0},{1}", X, Y);
		}

		public static string ConvertToString(int x, int y)
		{
			return string.Format("{0},{1}", x, y);
		}
	}
}