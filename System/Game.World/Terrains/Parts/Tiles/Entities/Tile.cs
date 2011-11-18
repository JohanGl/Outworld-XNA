using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Parts.Tiles
{
	/// <summary>
	/// Defines a 3d map tile
	/// </summary>
	public struct Tile
	{
		public static Vector3 Size = new Vector3(1, 1, 1);
		public static Vector3 HalfSize = new Vector3(0.5f, 0.5f, 0.5f);

		/// <summary>
		/// Defines the tile type (sand, grass, stone etc)
		/// </summary>
		public TileType Type;

		public override string ToString()
		{
			return string.Format("Type: {0}", Type);
		}
	}
}