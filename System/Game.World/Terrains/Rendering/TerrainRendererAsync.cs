using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Framework.Core.Diagnostics.Logging;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Areas.AreaModels;

namespace Game.World.Terrains.Rendering
{
	/// <summary>
	/// Handles rendering of terrain areas
	/// </summary>
	public partial class TerrainRenderer
	{
		protected class ThreadingContext
		{
			public static bool Running;
			public static Thread Worker;
			public static AddAsyncJob Job;
			public static AddAsyncResult Result;
			public static Stopwatch Stopwatch;

			public static void Start(ThreadStart threadStart)
			{
				Job = new AddAsyncJob();
				Stopwatch = new Stopwatch();

				Worker = new Thread(threadStart)
				{
					Name = "TerrainRenderer.ThreadingContext",
					IsBackground = true
				};

				Worker.Priority = ThreadPriority.AboveNormal;
				Worker.Start();
			}
		}

		public bool AddAsyncCompleted
		{
			get
			{
				return ThreadingContext.Job.Status == AddAsyncJobStatus.Completed;
			}
		}

		public AddAsyncResult AddAsyncResult
		{
			get
			{
				return ThreadingContext.Result;
			}
		}

		public void AddAsync(string key, List<Area> areas, List<Area> areasCache, bool clearAllCurrentAreas, object tag = null)
		{
			var newAreas = new List<Area>();

			for (int i = 0; i < areas.Count; i++)
			{
				bool add = true;

				// Check if the current area already exists
				for (int j = 0; j < terrainVisibility.AreaCollection.Areas.Count; j++)
				{
					if (areas[i].Info.LocationId == terrainVisibility.AreaCollection.Areas[j].Info.LocationId)
					{
						add = false;
						break;
					}
				}

				// The current area does not exist yet
				if (add)
				{
					newAreas.Add(areas[i]);
				}
			}

			if (newAreas.Count == 0)
			{
				return;
			}

			if (ThreadingContext.Job.Status == AddAsyncJobStatus.Completed ||
				ThreadingContext.Job.Status == AddAsyncJobStatus.Unknown)
			{
				// Check if we should remove all existing areas before adding these new ones
				if (clearAllCurrentAreas)
				{
					Clear();
				}

				ThreadingContext.Job.Key = key;
				ThreadingContext.Job.Tag = tag;
				ThreadingContext.Job.Areas = newAreas;
				ThreadingContext.Job.AreasCache = areasCache.Concat(terrainVisibility.AreaCollection.Areas).ToList();
				ThreadingContext.Job.Status = AddAsyncJobStatus.ReadyToStart;

				ThreadingContext.Stopwatch.Start();
				Logger.Log<TerrainRenderer>(LogLevel.Debug, "AddAsync: Started");
			}
		}

		private void AddAsyncLogic()
		{
			ThreadingContext.Running = true;

#if XBOX
			Thread.CurrentThread.SetProcessorAffinity(new int[] { 3 });
#endif

			Area currentArea;

			while (ThreadingContext.Running)
			{
				// If we have a new job
				if (ThreadingContext.Job != null &&
					ThreadingContext.Job.Status == AddAsyncJobStatus.ReadyToStart)
				{
					// Lock the job exclusively to this thread instance
					lock (ThreadingContext.Job)
					{
						ThreadingContext.Job.Status = AddAsyncJobStatus.Running;

						ThreadingContext.Result = new AddAsyncResult()
						{
							Key = ThreadingContext.Job.Key,
							Tag = ThreadingContext.Job.Tag,
							Areas = new List<Area>()
						};

						// Lock the job result exclusively to this thread instance
						lock (ThreadingContext.Result)
						{
							// Combine all areas used when finding area neighbors
							var totalAreas = ThreadingContext.Job.Areas.Concat(ThreadingContext.Job.AreasCache).ToList();

							var neighbors = new List<Area>();

							// Loop through all areas to be rendered
							for (int i = 0; i < ThreadingContext.Job.Areas.Count; i++)
							{
								currentArea = ThreadingContext.Job.Areas[i];

								neighbors.Clear();

								// Get all neighbours of the current area
								for (int j = 0; j < totalAreas.Count; j++)
								{
									if (totalAreas[j].Info.Location.X >= currentArea.Info.Location.X - 1 &&
										totalAreas[j].Info.Location.X <= currentArea.Info.Location.X + 1 &&
										totalAreas[j].Info.Location.Y >= currentArea.Info.Location.Y - 1 &&
										totalAreas[j].Info.Location.Y <= currentArea.Info.Location.Y + 1 &&
										totalAreas[j].Info.Location.Z >= currentArea.Info.Location.Z - 1 &&
										totalAreas[j].Info.Location.Z <= currentArea.Info.Location.Z + 1)
									{
										if (totalAreas[j].Info.LocationId != currentArea.Info.LocationId)
										{
											neighbors.Add(totalAreas[j]);
										}
									}
								}

								// Create a renderable area based on the current area
								AreaModelHandler.Build(currentArea, neighbors);
								ThreadingContext.Result.Areas.Add(currentArea);
							}
						}

						ThreadingContext.Stopwatch.Stop();
						Logger.Log<TerrainRenderer>(LogLevel.Debug, "AddAsync: Completed in {0} ms", ThreadingContext.Stopwatch.ElapsedMilliseconds);
						ThreadingContext.Stopwatch.Reset();

						ThreadingContext.Job.Status = AddAsyncJobStatus.Completed;
					}
				}

				// Wait for more work in intervals so we dont waste unnecessary resources
				Thread.Sleep(1);
			}
		}
	}

	public class AddAsyncJob
	{
		public string Key;
		public object Tag;
		public AddAsyncJobStatus Status;
		public List<Area> Areas;
		public List<Area> AreasCache;
	}

	public class AddAsyncResult
	{
		public string Key;
		public object Tag;
		public List<Area> Areas;
	}

	public enum AddAsyncJobStatus
	{
		Unknown,
		ReadyToStart,
		Running,
		Completed
	}
}