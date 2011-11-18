using Framework.Core.Common;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Parts.Tiles;
using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Parts.Areas
{
	public class AreaInfo
	{
		public NoiseAreaType Type;

		public Tile[] Tiles;

		private Vector3i location;

		/// <summary>
		/// The location of the area within the world
		/// </summary>
		public Vector3i Location
		{
			get
			{
				return location;
			}

			set
			{
				location = value;
				UpdateLocationDependentProperties();
			}
		}

		public string LocationId { get; private set; }

		public Range<int> TileHeight { get; private set; }

		/// <summary>
		/// The center point of the area in world coordinates
		/// </summary>
		public Vector3 Center { get; private set; }

		/// <summary>
		/// The bounding box of the area in world coordinates
		/// </summary>
		public BoundingBox BoundingBox { get; private set; }

		/// <summary>
		/// Gets or sets the flag indicating whether all tiles are empty or not
		/// </summary>
		public bool IsEmpty;

		public AreaInfo()
		{
			Tiles = new Tile[Area.TotalTiles];
			TileHeight = new Range<int>();
			IsEmpty = true;
		}

		/// <summary>
		/// Clears all tile information of the area.
		/// Note that location, boundingbox and center has to be handled separately during re-initialization due to performance improvements.
		/// </summary>
		public void Clear()
		{
			// Clear all tiles
			for (int i = 0; i < Tiles.Length; i++)
			{
				Tiles[i].Type = TileType.Empty;
			}

			// Mark the area as empty
			IsEmpty = true;
		}

		private void UpdateLocationDependentProperties()
		{
			// String identifier
			LocationId = location.ToString();

			// Tile range on the vertical axis
			TileHeight.Start = location.Y * Area.Size.Y;
			TileHeight.End = TileHeight.Start + Area.Size.Y;

			// Calculate the area offset within the world
			var offset = new Vector3(location.X * Area.Size.X,
									(location.Y * Area.Size.Y) + Area.Size.Y,
									 location.Z * Area.Size.Z);

			// Calculate the center point of the area
			Center = new Vector3(offset.X + (Tile.Size.X * Area.HalfSize.X),
								 offset.Y + -(Tile.Size.Y * Area.HalfSize.Y),
								 offset.Z + (Tile.Size.Z * Area.HalfSize.Z));

			// Calculate the bounding box of the area
			BoundingBox = new BoundingBox(new Vector3(offset.X,
													  offset.Y - (Tile.Size.Y * Area.Size.Y),
													  offset.Z),
										  new Vector3(offset.X + (Tile.Size.X * Area.Size.X),
													  offset.Y,
													  offset.Z + (Tile.Size.Z * Area.Size.Z)));
		}
	}
}