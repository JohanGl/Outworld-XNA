namespace Game.World.Terrains.Parts.Tiles
{
	/// <summary>
	/// Defines all available sides of a tile
	/// </summary>
	public enum TileSide : byte
	{
		Top = 0,
		Bottom,
		Left,
		Right,
		Front,
		Back
	}

	/// <summary>
	/// Defines all available tile types such as material
	/// </summary>
	public enum TileType : byte
	{
		Empty = 0,
		Grass,
		Grass2,
		Stone,
		Stone2,
		Sand,
		Mud
	}
}