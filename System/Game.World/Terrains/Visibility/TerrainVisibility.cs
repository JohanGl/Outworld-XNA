using System.Collections.Generic;
using System.Linq;
using Framework.Core.Common;
using Framework.Core.Diagnostics.Logging;
using Framework.Core.Scenes.Cameras;
using Game.World.Terrains.Contexts;
using Game.World.Terrains.Helpers;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Areas.AreaModels;
using Game.World.Terrains.Parts.Areas.Collections;
using Game.World.Terrains.Parts.Areas.Helpers;
using Game.World.Terrains.Rendering.MeshPools;
using Game.World.Terrains.Visibility.Helpers;
using Game.World.Terrains.Visibility.Queues;
using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Visibility
{
	public partial class TerrainVisibility
	{
		#region Members

		public AreaCollection AreaCollection;
		public AreaCacheCollection AreaCache;
		public TerrainVisibilityStatistics Statistics;
		public AreaSpawnPointHelper SpawnPointHelper;

		private TerrainContext terrainContext;
		private ViewSettings view;
		private VisibilityQueue visibilityQueue;
		private AreaRange viewDistanceAreaRange;
		private float areasAlwaysVisibleWithinDistance;
		private float areaToCameraDistance;
		private Vector3 areaLookAtNormal;

		/// <summary>
		/// Reused variable within this class used to minimize GC calls
		/// </summary>
		private AreaRange areaRange;

		#endregion
		
		public void Initialize(Vector2i viewDistance, TerrainContext terrainContext)
		{
			this.terrainContext = terrainContext;

			areasAlwaysVisibleWithinDistance = Area.Size.X * 2;

			view = new ViewSettings(viewDistance);

			AreaCollection = new AreaCollection();
			AreaCache = new AreaCacheCollection();
			areaRange = new AreaRange();
			viewDistanceAreaRange = new AreaRange();

			visibilityQueue = new VisibilityQueue(terrainContext);
			Statistics = new TerrainVisibilityStatistics(this, AreaCache);
			SpawnPointHelper = new AreaSpawnPointHelper(terrainContext);

			Logger.RegisterLogLevelsFor<TerrainVisibility>(Logger.LogLevels.Adaptive);
		}

		public void Clear()
		{
			terrainContext.Generator.ReInitialize();
			AreaCache.Clear();
			AreaCollection.Clear();
			Statistics.UpdateStatistics();
		}

		public void Teleport(Vector3 position)
		{
			// Clear all current areas
			Clear();

			// Translate the world position to an area location
			AreaHelper.FindAreaLocation(position, ref view.Location);

			// Get the area range around the teleported location
			AreaHelper.GetAreaRangeWithinViewDistance(view.Location, view.ViewDistanceCache, ref areaRange);

			// Iterate all areas nearby the requested teleport location
			for (int x = areaRange.Min.X; x <= areaRange.Max.X; x++)
			{
				for (int z = areaRange.Min.Z; z <= areaRange.Max.Z; z++)
				{
					for (int y = areaRange.Min.Y; y <= areaRange.Max.Y; y++)
					{
						// Generate the area
						var area = terrainContext.Generator.Generate(x, y, z);

						// For outer boundaries, add a cached area
						if (areaRange.IsAtEdge(x, y, z))
						{
							AreaCache.Add(area);
						}
						// For inner areas, add a visible area
						else
						{
							AreaCollection.Add(area);
						}
					}
				}
			}

			// Update the outer bounds
			AreaHelper.GetAreaRangeBoundaries(ref viewDistanceAreaRange, AreaCollection.Areas);

			// Render the area meshes
			TerrainMeshPool.Clear();

			for (int i = 0; i < AreaCollection.Areas.Count; i++)
			{
				AreaModelHandler.Build(AreaCollection.Areas[i]);
			}

			Logger.Log<TerrainVisibility>(LogLevel.Debug, "Teleported to {0} with a view distance of {1}", view.Location, view.ViewDistance.ToString());
		}

		public void Update(ref Vector3 position)
		{
			UpdateCurrentJob();

			// Translate the player world coordinates to an area location
			AreaHelper.FindAreaLocation(position, ref view.Location);

			if (view.IsWithinSameLocationAsPreviously)
			{
				return;
			}

			ExpandVisibility();

			// Update the previous location to the new one
			view.PreviousLocation = view.Location;
	
			AreaCache.RemoveExpiredAreas(view.Location, view.ViewDistance);

			Statistics.UpdateStatistics();
		}

		private void UpdateCurrentJob()
		{
			// Updates the current queue job running
			bool completedCurrentJob = visibilityQueue.Update();

			if (completedCurrentJob)
			{
				// Update the current area bounds
				AreaHelper.GetAreaRangeBoundaries(ref viewDistanceAreaRange, AreaCollection.Areas);
			}
		}

		private void ExpandVisibility()
		{
			AreaHelper.GetAreaRangeWithinViewDistance(view.Location, view.ViewDistanceCache, ref areaRange);

			// Remove any garbage areas outside the view area
			var areasOutsideRange = new List<Area>();
			AreaHelper.GetAreasOutsideRange(ref areasOutsideRange, AreaCollection.Areas, areaRange);

			Logger.Log<TerrainVisibility>(LogLevel.Debug, "Caching {0} rendered areas", areasOutsideRange.Count);

			MoveVisibleAreasToCache(areasOutsideRange);

			Logger.Log<TerrainVisibility>(LogLevel.Debug, "Entering area {0} at location {1}", areaRange, view.Location);

			var job = new VisibilityQueueJob(areaRange);

			// Iterate all coordinates of the current view distance (including cached areas at the edge)
			for (int x = areaRange.Min.X; x <= areaRange.Max.X; x++)
			{
				for (int z = areaRange.Min.Z; z <= areaRange.Max.Z; z++)
				{
					for (int y = areaRange.Min.Y; y <= areaRange.Max.Y; y++)
					{
						var cachedArea = AreaCache.GetAreaAt(x, y, z);

						// If the current coordinates are located at the non-visible cached area edge
						if (areaRange.IsAtEdge(x, y, z))
						{
							var visibleArea = AreaCollection.GetAreaAt(x, y, z);

							// Cache visible areas at the edge
							if (visibleArea != null)
							{
								MoveVisibleAreaToCache(visibleArea);
							}
							// Generate the area if missing
							else if (cachedArea == null)
							{
								job.AreasToGenerate.Add(new Vector3i(x, y, z));
							}
						}
						// If the current coordinates are located within the visible area
						else
						{
							// Turn cached areas into visible areas
							if (cachedArea != null)
							{
								job.AreasToRender.Add(cachedArea);
							}
						}
					}
				}
			}

			visibilityQueue.Add(job);
		}

		private void MoveVisibleAreaToCache(Area area)
		{
			area.Model.Clear();
			AreaCollection.Remove(area);
			AreaCache.Add(area);
		}

		private void MoveVisibleAreasToCache(List<Area> areas)
		{
			for (int i = 0; i < areas.Count; i++)
			{
				areas[i].Model.Clear();
			}

			AreaCache.AddRange(areas);
			AreaCollection.RemoveRange(areas);
		}

		/// <summary>
		/// Calculates if the area is visible within a camera viewport or not
		/// </summary>
		/// <param name="area">The area</param>
		/// <param name="camera">The camera to check against</param>
		/// <returns>True if the area is visible, false if not</returns>
		public bool IsAreaVisible(Area area, CameraBase camera)
		{
			if (area.Info.IsEmpty)
			{
				return false;
			}

			// Calculate the distance from the area to the camera
			areaToCameraDistance = (area.Info.Center - camera.Position).Length();

			// Is the area within stand-on or next-to distance
			if (areaToCameraDistance < areasAlwaysVisibleWithinDistance)
			{
				return true;
			}

			// Calculate the normal from the area pointing towards the camera position
			areaLookAtNormal = (camera.Position - area.Info.Center);
			areaLookAtNormal.Normalize();

			return Vector3.Dot(camera.LookAtNormal, areaLookAtNormal) <= -0.5f;
		}

		public List<Area> GetAreaNeighbors(Area area)
		{
			lock (AreaCollection.CollectionLock)
			{
				var neighbors = new List<Area>();

				// Get all neighbours of the current area
				foreach (var neighbor in AreaCollection.Areas.Concat(AreaCache.Areas))
				{
					if (neighbor.Info.Location.X >= area.Info.Location.X - 1 &&
						neighbor.Info.Location.X <= area.Info.Location.X + 1 &&
						neighbor.Info.Location.Y >= area.Info.Location.Y - 1 &&
						neighbor.Info.Location.Y <= area.Info.Location.Y + 1 &&
						neighbor.Info.Location.Z >= area.Info.Location.Z - 1 &&
						neighbor.Info.Location.Z <= area.Info.Location.Z + 1)
					{
						if (neighbor.Info.LocationId != area.Info.LocationId)
						{
							neighbors.Add(neighbor);
						}
					}
				}

				return neighbors;
			}
		}

		public Area GetAreaAt(int x, int y, int z)
		{
			string locationId = Vector3i.ConvertToString(x, y, z);

			for (int i = 0; i < AreaCollection.Areas.Count; i++)
			{
				if (AreaCollection.Areas[i].Info.LocationId == locationId)
				{
					return AreaCollection.Areas[i];
				}
			}

			return null;
		}
	}
}