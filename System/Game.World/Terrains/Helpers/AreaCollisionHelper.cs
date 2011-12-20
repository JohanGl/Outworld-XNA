using System;
using System.Collections.Generic;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Tiles;
using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Helpers
{
	/// <summary>
	/// Handles collision operations for single Areas
	/// </summary>
	public class AreaCollisionHelper
	{
		private Area collisionArea;
		private List<CollisionTile> collisionTiles;
		private List<Vector3> boxCollisionPoints;

		public AreaCollisionHelper()
		{
			collisionTiles = new List<CollisionTile>();
			boxCollisionPoints = new List<Vector3>();
		}

		public AreaCollisionHelper(Area area)
		{
			collisionTiles = new List<CollisionTile>();
			boxCollisionPoints = new List<Vector3>();

			SetArea(area);
		}

		/// <summary>
		/// Sets the area to perform collision tests with
		/// </summary>
		/// <param name="area"></param>
		public void SetArea(Area area)
		{
			collisionArea = area;
		}
		
		public List<CollisionTile> GetIntersectingTiles(BoundingBox box)
		{
			boxCollisionPoints.Clear();
			collisionTiles.Clear();

			UpdateBoxCollisionPoints(ref box);
			FindCollidingTiles(collisionArea, ref boxCollisionPoints);

			// Return the colliding tiles
			return collisionTiles;
		}

		private void UpdateBoxCollisionPoints(ref BoundingBox box)
		{
			for (float y = box.Min.Y; y <= box.Max.Y; y += Tile.Size.Y)
			{
				for (float z = box.Min.Z; z <= box.Max.Z; z += Tile.Size.Z)
				{
					for (float x = box.Min.X; x <= box.Max.X; x += Tile.Size.X)
					{
						boxCollisionPoints.Add(new Vector3(x, y, z));
					}
				}
			}
		}

		private void FindCollidingTiles(Area area, ref List<Vector3> points)
		{
			// Loop through all corners of the bounding box
			for (int i = 0; i < points.Count; i++)
			{
				// If the current point is inside the area
				if (area.Info.BoundingBox.Contains(points[i]) == ContainmentType.Contains)
				{
					// Calculate the point indices within the area tile array
					int x = Math.Min((Area.Size.X - 1), (int)(points[i].X - area.Info.BoundingBox.Min.X));
					int y = Math.Min((Area.Size.Y - 1), (int)(area.Info.BoundingBox.Max.Y - points[i].Y));
					int z = Math.Min((Area.Size.Z - 1), (int)(points[i].Z - area.Info.BoundingBox.Min.Z));

					// Get the tile
					var tile = area.Info.Tiles[(Area.LevelSize * y) + (z * Area.Size.X) + x];

					// If the tile is visible and hasnt been added yet
					if (tile.Type != TileType.Empty && !CollisionTilesContains(x, y, z))
					{
						var result = new CollisionTile()
						{
							Id = collisionTiles.Count.ToString(),
							Tile = tile,
							BoundingBox = new BoundingBox(new Vector3(area.Info.BoundingBox.Min.X + x * Tile.Size.X,
																	  area.Info.BoundingBox.Max.Y - ((y + 1) * Tile.Size.Y),
																	  area.Info.BoundingBox.Min.Z + z * Tile.Size.Z),
														  new Vector3((area.Info.BoundingBox.Min.X + x * Tile.Size.X) + Tile.Size.X,
																	  area.Info.BoundingBox.Max.Y - (y * Tile.Size.Y),
																	  (area.Info.BoundingBox.Min.Z + z * Tile.Size.Z) + Tile.Size.Z))
						};

						collisionTiles.Add(result);
					}
				}
			}
		}

		private bool CollisionTilesContains(float x, float y, float z)
		{
			for (int i = 0; i < collisionTiles.Count; i++)
			{
				if (collisionTiles[i].BoundingBox.Min.X == x &&
					collisionTiles[i].BoundingBox.Min.Y == y &&
					collisionTiles[i].BoundingBox.Min.Z == z)
				{
					return true;
				}
			}

			return false;
		}
	}
}