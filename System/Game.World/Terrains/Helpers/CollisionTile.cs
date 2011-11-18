using Game.World.Terrains.Parts.Tiles;
using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Helpers
{
	public struct CollisionTile
	{
		public string Id;
		public Tile Tile;
		public BoundingBox BoundingBox;
	}
}