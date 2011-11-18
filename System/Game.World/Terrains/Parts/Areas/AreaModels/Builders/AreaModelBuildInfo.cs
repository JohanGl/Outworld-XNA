using System.Collections.Generic;
using Framework.Core.Common;
using Game.World.Terrains.Parts.Tiles;

namespace Game.World.Terrains.Parts.Areas.AreaModels.Builders
{
	public class AreaModelBuildInfo
	{
		public Vector3i AreaLocationMin;
		public Vector3i AreaLocationMax;
		public Area[] AreaNeighbors;
		public bool[] TileVisibleSides;

		private Area area;

		public void Initialize(Area area, List<Area> neighbors)
		{
			this.area = area;
			AreaNeighbors = new Area[6];

			UpdateAreaNeighbors(neighbors);
			UpdateAreaBoundaries();

			TileVisibleSides = new bool[6];
		}

		private void UpdateAreaNeighbors(List<Area> neighbors)
		{
			// Clear any current areas
			for (int i = 0; i < 6; i++)
			{
				AreaNeighbors[i] = null;
			}

			if (neighbors != null && neighbors.Count > 0)
			{
				var sides = new Vector3i[6];
				sides[(int)TileSide.Top] = new Vector3i(area.Info.Location.X, area.Info.Location.Y + 1, area.Info.Location.Z);
				sides[(int)TileSide.Bottom] = new Vector3i(area.Info.Location.X, area.Info.Location.Y - 1, area.Info.Location.Z);
				sides[(int)TileSide.Left] = new Vector3i(area.Info.Location.X - 1, area.Info.Location.Y, area.Info.Location.Z);
				sides[(int)TileSide.Right] = new Vector3i(area.Info.Location.X + 1, area.Info.Location.Y, area.Info.Location.Z);
				sides[(int)TileSide.Front] = new Vector3i(area.Info.Location.X, area.Info.Location.Y, area.Info.Location.Z + 1);
				sides[(int)TileSide.Back] = new Vector3i(area.Info.Location.X, area.Info.Location.Y, area.Info.Location.Z - 1);

				for (int i = 0; i < neighbors.Count; i++)
				{
					for (int j = 0; j < 6; j++)
					{
						if (neighbors[i].Info.Location.Equals(sides[j]))
						{
							AreaNeighbors[j] = neighbors[i];
							break;
						}
					}
				}
			}
		}

		private void UpdateAreaBoundaries()
		{
			bool updatedBoundaries = false;

			// Locate the outer boundaries (locations) of all neighbor areas so that we can skip rendering faces which faces out of the furthest areas (cannot be seen anyway)
			AreaLocationMin = new Vector3i(int.MaxValue);
			AreaLocationMax = new Vector3i(int.MinValue);

			// Loop through all sides of the current area
			for (int i = 0; i < 6; i++)
			{
				if (AreaNeighbors[i] != null)
				{
					updatedBoundaries = true;

					// X axis
					if (AreaNeighbors[i].Info.Location.X < AreaLocationMin.X)
					{
						AreaLocationMin.X = AreaNeighbors[i].Info.Location.X;
					}

					if (AreaNeighbors[i].Info.Location.X > AreaLocationMax.X)
					{
						AreaLocationMax.X = AreaNeighbors[i].Info.Location.X;
					}

					// Y axis
					if (AreaNeighbors[i].Info.Location.Y < AreaLocationMin.Y)
					{
						AreaLocationMin.Y = AreaNeighbors[i].Info.Location.Y;
					}

					if (AreaNeighbors[i].Info.Location.Y > AreaLocationMax.Y)
					{
						AreaLocationMax.Y = AreaNeighbors[i].Info.Location.Y;
					}

					// Z axis
					if (AreaNeighbors[i].Info.Location.Z < AreaLocationMin.Z)
					{
						AreaLocationMin.Z = AreaNeighbors[i].Info.Location.Z;
					}

					if (AreaNeighbors[i].Info.Location.Z > AreaLocationMax.Z)
					{
						AreaLocationMax.Z = AreaNeighbors[i].Info.Location.Z;
					}
				}
			}

			// No neighbors found, so use default boundaries
			if (!updatedBoundaries)
			{
				AreaLocationMin = area.Info.Location;
				AreaLocationMax = area.Info.Location;
			}
		}
	}
}