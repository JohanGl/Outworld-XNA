using Framework.Core.Common;
using Framework.Core.Helpers;
using Game.World.Terrains.Contexts;
using Game.World.Terrains.Helpers;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Areas.Helpers;
using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Visibility.Helpers
{
	public class AreaSpawnPointHelper
	{
		/// <summary>
		/// Stores spawnpoint searchresult info
		/// </summary>
		protected class SpawnPointInfo
		{
			public bool IsAtBottomOfArea;
			public Vector3 Position;
		}

		private TerrainContext terrainContext;
		private BoundingBoxHelper boundingBoxHelper;
		private AreaCollisionHelper areaCollisionHelper;
		private SpawnPointInfo info;

		public AreaSpawnPointHelper(TerrainContext terrainContext)
		{
			this.terrainContext = terrainContext;
			boundingBoxHelper = new BoundingBoxHelper();
			areaCollisionHelper = new AreaCollisionHelper();
			info = new SpawnPointInfo();
		}

		public Vector3 FindSuitableSpawnPoint(BoundingBox bounds)
		{
			var currentBestPosition = new Vector3();

			// Find the area location of the current bounding box coordinates
			var startingArea = new Vector3i();
			AreaHelper.FindAreaLocation(boundingBoxHelper.GetCenter(bounds), ref startingArea);

			// Check the current area for a suitable spawnpoint
			var area = terrainContext.Generator.Generate(startingArea.X, startingArea.Y, startingArea.Z);

			if (CanSpawnInArea(area, bounds))
			{
				// If we are at the bottom of an area, then theres a chance that the area below us is also empty and might cause us to start falling.
				// If so, we try to scan further for a suitable position, otherwise we return right here since we can assume that we are standing on solid ground.
				if (!info.IsAtBottomOfArea)
				{
					return info.Position;
				}
				else
				{
					currentBestPosition = info.Position;
				}
			}

			bool skipAbove = currentBestPosition != Vector3.Zero;

			// Perform a scan of 50 areas above and below the starting area for a suitable spawnpoint
			for (int y = 1; y < 50; y++)
			{
				if (!skipAbove)
				{
					var areaAbove = terrainContext.Generator.Generate(startingArea.X, startingArea.Y + y, startingArea.Z);

					if (CanSpawnInArea(areaAbove, bounds))
					{
						// If we are at the bottom of an area, then theres a chance that the area below us is also empty and might cause us to start falling.
						// If so, we try to scan further for a suitable position, otherwise we return right here since we can assume that we are standing on solid ground.
						if (!info.IsAtBottomOfArea)
						{
							return info.Position;
						}

						currentBestPosition = info.Position;
					}
				}

				var areaBelow = terrainContext.Generator.Generate(startingArea.X, startingArea.Y - y, startingArea.Z);

				if (CanSpawnInArea(areaBelow, bounds))
				{
					// If we are at the bottom of an area, then theres a chance that the area below us is also empty and might cause us to start falling.
					// If so, we try to scan further for a suitable position, otherwise we return right here since we can assume that we are standing on solid ground.
					if (!info.IsAtBottomOfArea)
					{
						return info.Position;
					}

					currentBestPosition = info.Position;
				}
			}

			return currentBestPosition;
		}

		private bool CanSpawnInArea(Area area, BoundingBox boundingBox)
		{
			// Handle empty areas
			if (area.Info.IsEmpty)
			{
				SnapBoundingBoxToBottomOfArea(area, ref boundingBox);
				info.IsAtBottomOfArea = true;
				info.Position = boundingBoxHelper.GetCenter(boundingBox);
				return true;
			}

			areaCollisionHelper.SetArea(area);

			float boundsHeight = boundingBox.Max.Y - boundingBox.Min.Y;

			// Loop through all tiles in the area and perform collision checks against the bounding box, looping from the bottom of the area and up
			for (int y = 0; y < Area.Size.Y; y++)
			{
				int worldY = area.Info.Location.Y * Area.Size.Y;

				// Adjust the bounding box height
				boundingBox.Min.Y = worldY + y;
				boundingBox.Max.Y = boundingBox.Min.Y + boundsHeight;

				// Get all intersecting tiles for the bounding box at the current height
				var intersectingTiles = areaCollisionHelper.GetIntersectingTiles(boundingBox);

				// Found a suitable position?
				if (intersectingTiles.Count == 0)
				{
					//ImageExporter.AreaSliceOnXAxisToBitmap("AreaSpawnPointHelper_Plains.png", area, 16);

					info.IsAtBottomOfArea = (y == 0);
					info.Position = boundingBoxHelper.GetCenter(boundingBox);
					return true;
				}
			}

			// Failed to find a suitable spawnpoint in this area
			return false;
		}

		private void SnapBoundingBoxToBottomOfArea(Area area, ref BoundingBox boundingBox)
		{
			float boundsHeight = boundingBox.Max.Y - boundingBox.Min.Y;

			int worldY = area.Info.Location.Y * Area.Size.Y;

			boundingBox.Min.Y = worldY;
			boundingBox.Max.Y = boundingBox.Min.Y + boundsHeight;
		}
	}
}