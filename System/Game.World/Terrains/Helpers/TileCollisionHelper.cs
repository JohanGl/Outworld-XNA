using System;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Tiles;
using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Helpers
{
	public class TileCollisionHelper
	{
		public Tile GetIntersectingTile(Area area, Vector3 point)
		{
			// If the current point is inside the area
			if (area.Info.BoundingBox.Contains(point) == ContainmentType.Contains)
			{
				System.Diagnostics.Debug.WriteLine("GetIntersectingTile IF-statement TRUE");

				// Calculate the point indices within the area tile array
				int x = Math.Min((Area.Size.X - 1), (int)(point.X - area.Info.BoundingBox.Min.X));
				int y = Math.Min((Area.Size.Y - 1), (int)(area.Info.BoundingBox.Max.Y - point.Y));
				int z = Math.Min((Area.Size.Z - 1), (int)(point.Z - area.Info.BoundingBox.Min.Z));
				
				// Get the tile
				return area.Info.Tiles[(Area.LevelSize * y) + (z * Area.Size.X) + x];
			}

			System.Diagnostics.Debug.WriteLine("GetIntersectingTile IF-statement FALSE");

			var resultTile = new Tile();
			resultTile.Type = TileType.Empty;
			
			return resultTile;
		}
	}
}
