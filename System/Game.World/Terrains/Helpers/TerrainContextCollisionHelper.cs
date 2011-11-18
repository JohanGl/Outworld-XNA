using System;
using System.Collections.Generic;
using Game.World.Terrains.Contexts;
using Game.World.Terrains.Parts.Areas;
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

	/// <summary>
	/// Handles collision operations for all areas within a TerrainContext
	/// </summary>
	public class TerrainContextCollisionHelper
	{
		private TerrainContext terrainContext;
		private List<Area> collisionAreas;
		private List<CollisionTile> collisionTiles;
		private List<Vector3> boxCollisionPoints;

		public TerrainContextCollisionHelper(TerrainContext terrainContext)
		{
			this.terrainContext = terrainContext;

			collisionAreas = new List<Area>();
			collisionTiles = new List<CollisionTile>();
			boxCollisionPoints = new List<Vector3>();
		}

		public List<CollisionTile> GetIntersectingTiles(BoundingBox box)
		{
			UpdateIntersectingCollisionAreas(ref box);
			UpdateBoxCollisionPoints(ref box);

			// Clear the previous results
			collisionTiles.Clear();

			for (int i = 0; i < collisionAreas.Count; i++)
			{
				FindCollidingTiles(collisionAreas[i], ref boxCollisionPoints);
			}

			// Return the colliding tiles
			return collisionTiles;
		}

		private void UpdateIntersectingCollisionAreas(ref BoundingBox box)
		{
			collisionAreas.Clear();

			// Find the areas which the box intersects
			for (int i = 0; i < terrainContext.Visibility.AreaCollection.Areas.Count; i++)
			{
				var area = terrainContext.Visibility.AreaCollection.Areas[i];

				// Skip the area if empty
				if (area.Info.IsEmpty)
				{
					continue;
				}

				// If the current area intersects the box
				if (area.Info.BoundingBox.Intersects(box))
				{
					// Add the area to the list
					collisionAreas.Add(area);

					// If the box is fully contained within the first found area, then cancel searching for more areas
					if (collisionAreas.Count == 1 &&
						area.Info.BoundingBox.Contains(box) == ContainmentType.Contains)
					{
						break;
					}
				}
			}
		}

		private void UpdateBoxCollisionPoints(ref BoundingBox box)
		{
			// Clear the box collision points and build the new list
			boxCollisionPoints.Clear();

			for (float y = box.Min.Y; y < box.Max.Y; y += Tile.Size.Y)
			{
				for (float z = box.Min.Z; z < box.Max.Z; z += Tile.Size.Z)
				{
					for (float x = box.Min.X; x < box.Max.X; x += Tile.Size.X)
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

					int index = (Area.LevelSize * y) + (z * (int)Area.Size.Z) + x;

					// Get the tile
					var tile = area.Info.Tiles[index];

					// If the tile is visible and hasnt been added yet
					if (tile.Type != TileType.Empty && !CollisionTilesContains(x, y, z))
					{
						var result = new CollisionTile()
						{
							Id = index.ToString(),
							Tile = tile,
							BoundingBox = new BoundingBox(new Vector3(area.Info.BoundingBox.Min.X + x * Tile.Size.X,
																	  area.Info.BoundingBox.Max.Y - ((y + 1) * Tile.Size.Y),
																	  area.Info.BoundingBox.Min.Z + z * Tile.Size.Z),
														  new Vector3((area.Info.BoundingBox.Min.X + x * Tile.Size.X) + Tile.Size.X,
																	  area.Info.BoundingBox.Max.Y - (y * Tile.Size.Y),
																	  (area.Info.BoundingBox.Min.Z + z * Tile.Size.Z) + Tile.Size.Z))
						};

						result.BoundingBox.Min -= new Vector3(0.05f);
						result.BoundingBox.Max += new Vector3(0.05f);

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