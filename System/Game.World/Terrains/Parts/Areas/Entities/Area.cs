using Framework.Core.Common;

namespace Game.World.Terrains.Parts.Areas
{
	/// <summary>
	/// Defines a terrain area
	/// </summary>
	public class Area
	{
		#region Dimensions

		/// <summary>
		/// Defines the total number of tiles on each axis within the area
		/// </summary>
		public static readonly Vector3i Size = new Vector3i(32, 32, 32);

		/// <summary>
		/// Defines the half total number of tiles on each axis within the area
		/// </summary>
		public static readonly Vector3i HalfSize = new Vector3i(Size.X / 2, Size.Y / 2, Size.Z / 2);

		/// <summary>
		/// Defines the total number of tiles within the whole area
		/// </summary>
		public static readonly int TotalTiles = Size.X * Size.Y * Size.Z;

		/// <summary>
		/// Defines the total number of tiles for a single horizontal floor (x * z)
		/// </summary>
		public static readonly int LevelSize = Size.X * Size.Z;

		#endregion

		/// <summary>
		/// Describes the area spatial and tile related info
		/// </summary>
		public AreaInfo Info;

		/// <summary>
		/// Contains the mesh related info used for rendering the area
		/// </summary>
		public AreaModel Model;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Area()
		{
			Info = new AreaInfo();
			Model = new AreaModel();
		}

		/// <summary>
		/// Clears the area information
		/// </summary>
		public void Clear()
		{
			Info.Clear();
			Model.Clear();
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2}", Info.Location.X, Info.Location.Y, Info.Location.Z);
		}
	}
}