using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Framework.Core.Common;
using Framework.Core.Diagnostics.Logging;
using Game.World.Terrains.Parts.Areas;

namespace Game.World.Terrains.Generators
{
	/// <summary>
	/// Handles creation and loading of the terrain segments within the world
	/// </summary>
	public partial class TerrainGenerator
	{
		protected class ThreadingContext
		{
			public static bool Running;
			public static Thread Worker;
			public static GenerateAsyncJob Job;
			public static GenerateAsyncResult Result;
			public static Stopwatch Stopwatch;

			public static void Start(ThreadStart threadStart)
			{
				Job = new GenerateAsyncJob();
				Stopwatch = new Stopwatch();

				Worker = new Thread(threadStart)
				{
					Name = "TerrainGenerator.ThreadingContext",
					IsBackground = true
				};

				Worker.Priority = ThreadPriority.AboveNormal;
				Worker.Start();
			}
		}

		public bool GenerateAsyncCompleted
		{
			get
			{
				return ThreadingContext.Job.Status == GenerateAsyncJobStatus.Completed;
			}
		}

		public GenerateAsyncResult GenerateAsyncResult
		{
			get
			{
				return ThreadingContext.Result;
			}
		}

		public void GenerateAsync(string key, List<Vector3i> locations, object tag = null)
		{
			if (ThreadingContext.Job.Status == GenerateAsyncJobStatus.Completed ||
				ThreadingContext.Job.Status == GenerateAsyncJobStatus.Unknown)
			{
				ThreadingContext.Job.Key = key;
				ThreadingContext.Job.Tag = tag;
				ThreadingContext.Job.Locations = locations;
				ThreadingContext.Job.Status = GenerateAsyncJobStatus.ReadyToStart;

				ThreadingContext.Stopwatch.Start();
				Logger.Log<TerrainGenerator>(LogLevel.Debug, "GenerateAsync: Started");
			}
			else
			{
				throw new Exception("This should never happen");
			}
		}

		private void GenerateAsyncLogic()
		{
			ThreadingContext.Running = true;

#if XBOX
			Thread.CurrentThread.SetProcessorAffinity(new int[] { 3 });
#endif

			while (ThreadingContext.Running)
			{
				// If we have a new job
				if (ThreadingContext.Job != null &&
					ThreadingContext.Job.Status == GenerateAsyncJobStatus.ReadyToStart)
				{
					// Lock the job exclusively to this thread instance
					lock (ThreadingContext.Job)
					{
						ThreadingContext.Job.Status = GenerateAsyncJobStatus.Running;
						ThreadingContext.Result = new GenerateAsyncResult()
						{
							Key = ThreadingContext.Job.Key,
							Tag = ThreadingContext.Job.Tag,
							Areas = new List<Area>()
						};

						// Lock the job result exclusively to this thread instance
						lock (ThreadingContext.Result)
						{
							// Generate all areas
							foreach (var location in ThreadingContext.Job.Locations)
							{
								ThreadingContext.Result.Areas.Add(Generate(location.X, location.Y, location.Z));
							}
						}

						ThreadingContext.Job.Status = GenerateAsyncJobStatus.Completed;
					}

					ThreadingContext.Stopwatch.Stop();
					Logger.Log<TerrainGenerator>(LogLevel.Debug, "GenerateAsync: Completed in {0} ms", ThreadingContext.Stopwatch.ElapsedMilliseconds);
					ThreadingContext.Stopwatch.Reset();
				}

				// Wait for more work in intervals so we dont waste unnecessary resources
				Thread.Sleep(1);
			}
		}
	}

	public class GenerateAsyncJob
	{
		public string Key;
		public object Tag;
		public GenerateAsyncJobStatus Status;
		public List<Vector3i> Locations;
	}

	public class GenerateAsyncResult
	{
		public string Key;
		public object Tag;
		public List<Area> Areas;
	}

	public enum GenerateAsyncJobStatus
	{
		Unknown,
		ReadyToStart,
		Running,
		Completed
	}
}

#if XBOX
    // Processor affinity map.
    // Index CPU CORE Comment
    // -----------------------------------------------------------------------
    //   0    1    1  Please avoid using. (used by 360)
    //   1    1    2  Game runs here by default, so avoid this one too.
    //   2    2    1  Please avoid using. (used by 360)
    //   3    2    2  Part of Guide and Dashboard live here so usable in game.
    //   4    3    1  Live market place downloads use this so usable in game.
    //   5    3    2  Part of Guide and Dashboard live here so usable in game.
    // -----------------------------------------------------------------------  
#endif