using Framework.Core.Common;

namespace Game.World.Terrains.Visibility
{
	public partial class TerrainVisibility
	{
		private class ViewSettings
		{
			public Vector3i ViewDistance;
			public Vector3i ViewDistanceCache;
			public Vector3i Location;
			public Vector3i PreviousLocation;

			public ViewSettings(Vector2i viewDistance)
			{
				ViewDistance = new Vector3i(viewDistance.X, viewDistance.Y, viewDistance.X);
				ViewDistanceCache = new Vector3i(viewDistance.X + 1, viewDistance.Y + 1, viewDistance.X + 1);
				Location = new Vector3i();
				PreviousLocation = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
			}

			public bool IsWithinSameLocationAsPreviously
			{
				get
				{
					return (Location.X == PreviousLocation.X &&
							Location.Y == PreviousLocation.Y &&
							Location.Z == PreviousLocation.Z);
				}
			}
		}
	}
}