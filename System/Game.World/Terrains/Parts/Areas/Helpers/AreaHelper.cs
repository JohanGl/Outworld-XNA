using System;
using System.Collections.Generic;
using Framework.Core.Common;
using Game.World.Terrains.Parts.Tiles;
using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Parts.Areas.Helpers
{
	public static class AreaHelper
	{
		/// <summary>
		/// Takes a world position and finds the matching area location for it
		/// </summary>
		/// <param name="position">The world position</param>
		/// <param name="result">The result area location</param>
		public static void FindAreaLocation(Vector3 position, ref Vector3i result)
		{
			result.X = (int)Math.Ceiling(position.X / (Area.Size.X * Tile.Size.X)) - 1;
			result.Y = (int)Math.Ceiling(position.Y / (Area.Size.Y * Tile.Size.Y)) - 1;
			result.Z = (int)Math.Ceiling(position.Z / (Area.Size.Z * Tile.Size.Z)) - 1;
		}

		public static void GetAreasInRange(ref List<Area> result, IEnumerable<Area> areasToIterate, AreaRange range)
		{
			var iterator = areasToIterate.GetEnumerator();

			while (iterator.MoveNext())
			{
				// Within the bounds of the range?
				if (iterator.Current.Info.Location.X >= range.Min.X &&
					iterator.Current.Info.Location.X <= range.Max.X &&
					iterator.Current.Info.Location.Y >= range.Min.Y &&
					iterator.Current.Info.Location.Y <= range.Max.Y &&
					iterator.Current.Info.Location.Z >= range.Min.Z &&
					iterator.Current.Info.Location.Z <= range.Max.Z)
				{
					// Add it to our list and proceed with the next area
					result.Add(iterator.Current);
				}
			}
		}

		public static void GetAreasOutsideRange(ref List<Area> result, IEnumerable<Area> areasToIterate, AreaRange range)
		{
			var iterator = areasToIterate.GetEnumerator();

			while (iterator.MoveNext())
			{
				// Within the bounds of the range?
				if (iterator.Current.Info.Location.X >= range.Min.X && iterator.Current.Info.Location.X <= range.Max.X &&
					iterator.Current.Info.Location.Y >= range.Min.Y && iterator.Current.Info.Location.Y <= range.Max.Y &&
					iterator.Current.Info.Location.Z >= range.Min.Z && iterator.Current.Info.Location.Z <= range.Max.Z)
				{
				}
				// Outside the bounds of the range
				else
				{
					// Add it to our list and proceed with the next area
					result.Add(iterator.Current);
				}
			}
		}

		public static void GetAreaRangeBoundaries(ref AreaRange result, IEnumerable<Area> areasToIterate)
		{
			// No areas to process
			if (areasToIterate == null)
			{
				result = null;
				return;
			}

			var iterator = areasToIterate.GetEnumerator();

			// No areas to process
			if (!iterator.MoveNext())
			{
				result = null;
				return;
			}

			// Get the first area that we know exists in areasToIterate by now
			var first = iterator.Current;

			// Make sure that the result range is initialized before proceeding
			if (result == null)
			{
				result = new AreaRange();
			}

			// Reset the outer bounds
			result.Min.X = first.Info.Location.X;
			result.Min.Y = first.Info.Location.Y;
			result.Min.Z = first.Info.Location.Z;
			result.Max.X = first.Info.Location.X;
			result.Max.Y = first.Info.Location.Y;
			result.Max.Z = first.Info.Location.Z;

			// Loop through all areas and find the outer bounds
			foreach (var area in areasToIterate)
			{
				if (area.Info.Location.X < result.Min.X) result.Min.X = area.Info.Location.X;
				if (area.Info.Location.Y < result.Min.Y) result.Min.Y = area.Info.Location.Y;
				if (area.Info.Location.Z < result.Min.Z) result.Min.Z = area.Info.Location.Z;
				if (area.Info.Location.X > result.Max.X) result.Max.X = area.Info.Location.X;
				if (area.Info.Location.Y > result.Max.Y) result.Max.Y = area.Info.Location.Y;
				if (area.Info.Location.Z > result.Max.Z) result.Max.Z = area.Info.Location.Z;
			}
		}

		public static void GetAreaRangeWithinViewDistance(Vector3i location, Vector3i viewDistance, ref AreaRange range)
		{
			range.Min.X = location.X - viewDistance.X;
			range.Max.X = location.X + viewDistance.X;
			range.Min.Y = location.Y - viewDistance.Y;
			range.Max.Y = location.Y + viewDistance.Y;
			range.Min.Z = location.Z - viewDistance.Z;
			range.Max.Z = location.Z + viewDistance.Z;
		}

		public static int GetColumnHeight(Area area, int x, int z)
		{
			int height = Area.Size.Y - 1;

			// Loop through all tiles from top to bottom to find the height at the current x,z coordinates
			for (int y = 0; y < Area.Size.Y; y++)
			{
				int index = (z * Area.Size.X) + (y * Area.LevelSize) + x;

				if (area.Info.Tiles[index].Type != TileType.Empty)
				{
					break;
				}

				height--;
			}

			return height;
		}
	}
}