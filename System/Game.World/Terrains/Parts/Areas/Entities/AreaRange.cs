using Framework.Core.Common;

namespace Game.World.Terrains.Parts.Areas
{
	/// <summary>
	/// Defines a 3d range / bounding box used to store a span of areas
	/// </summary>
	public class AreaRange
	{
		public Vector3i Min;
		public Vector3i Max;

		public AreaRange()
		{
			Min = new Vector3i();
			Max = new Vector3i();
		}

		/// <summary>
		/// Checks if the specified coordinates are located at any of the min/max coordinates
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public bool IsAtEdge(int x, int y, int z)
		{
			return (x == Min.X || x == Max.X ||
					y == Min.Y || y == Max.Y ||
					z == Min.Z || z == Max.Z);
		}

		public override string ToString()
		{
			return string.Format("Min({0}), Max({1})", Min, Max);
		}
	}
}