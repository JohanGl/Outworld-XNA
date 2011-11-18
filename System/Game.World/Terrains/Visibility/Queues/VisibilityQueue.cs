using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Core.Diagnostics.Logging;
using Game.World.Terrains.Contexts;
using Game.World.Terrains.Parts.Areas;

namespace Game.World.Terrains.Visibility.Queues
{
	public class VisibilityQueue
	{
		private List<VisibilityQueueJob> queue;
		private TerrainContext terrainContext;

		private bool completedJob;
		private int batchIndex;
		private readonly int renderingAreasBatchSize;
		private DateTime pauseUntilNextBatch;

		private VisibilityQueueJob currentJob
		{
			get
			{
				return queue[0];
			}
		}

		public VisibilityQueue(TerrainContext terrainContext)
		{
			this.terrainContext = terrainContext;

			queue = new List<VisibilityQueueJob>();

			renderingAreasBatchSize = 10;
			pauseUntilNextBatch = DateTime.MinValue;

			Logger.RegisterLogLevelsFor<VisibilityQueue>(Logger.LogLevels.Adaptive);
		}

		public bool Update()
		{
			completedJob = false;

			// No more items in the queue
			if (queue.Count == 0)
			{
				return completedJob;
			}

			// If we are currently pausing before the next batch
			if (pauseUntilNextBatch != DateTime.MinValue)
			{
				// Has the pause duration expired?
				if (DateTime.Now >= pauseUntilNextBatch)
				{
					// Begin rendering the next batch
					pauseUntilNextBatch = DateTime.MinValue;
					BeginRenderingAreas(false);
				}

				return completedJob;
			}

			// Handle the current job status
			switch (currentJob.Status)
			{
				case VisibilityQueueJobStatus.Ready:
					BeginGeneratingAreas();
					break;

				case VisibilityQueueJobStatus.GeneratingAreas:
					GeneratingAreas();
					break;

				case VisibilityQueueJobStatus.RenderingAreas:
					RenderingAreas();
					break;
			}

			return completedJob;
		}

		private void BeginGeneratingAreas()
		{
			Logger.Log<VisibilityQueue>(LogLevel.Debug, "Generating areas started for range {0}", currentJob.Range);
		
			terrainContext.Generator.GenerateAsync("VisibilityQueue", currentJob.AreasToGenerate);
			currentJob.Status = VisibilityQueueJobStatus.GeneratingAreas;
		}

		private void GeneratingAreas()
		{
			if (terrainContext.Generator.GenerateAsyncCompleted)
			{
				terrainContext.Visibility.AreaCache.AddRange(terrainContext.Generator.GenerateAsyncResult.Areas);
				BeginRenderingAreas(true);
			}
		}

		private void BeginRenderingAreas(bool isFirstBatch)
		{
			if (isFirstBatch)
			{
				Logger.Log<VisibilityQueue>(LogLevel.Debug, "RenderingAreas Started");
				batchIndex = 0;
			}

			// Get the next batch to be rendered
			var areasToRender = currentJob.AreasToRender.Skip(batchIndex).Take(renderingAreasBatchSize).ToList();

			// Render the current batch
			if (areasToRender.Count > 0)
			{
				int batchNumber = (batchIndex != 0) ? (batchIndex / renderingAreasBatchSize) + 1 : 1;
				int totalBatches = (int)Math.Ceiling(currentJob.AreasToRender.Count / (double)renderingAreasBatchSize);

				Logger.Log<VisibilityQueue>(LogLevel.Debug, "Batch {0}/{1}, {2}-{3}", batchNumber, totalBatches, batchIndex, (batchIndex + areasToRender.Count));

				var possibleNeighbors = currentJob.AreasToRenderNeighbors.Concat(terrainContext.Visibility.AreaCache.Areas).ToList();

				terrainContext.Renderer.AddAsync("VisibilityQueue", areasToRender, possibleNeighbors, false);
				currentJob.Status = VisibilityQueueJobStatus.RenderingAreas;
			}
			else
			{
				// The job has completed so remove it from the queue
				queue.Remove(currentJob);
				completedJob = true;
			}
		}

		private void RenderingAreas()
		{
			if (terrainContext.Renderer.AddAsyncCompleted)
			{
				terrainContext.Visibility.AreaCollection.AddRange(terrainContext.Renderer.AddAsyncResult.Areas);

				// Proceed with the next batch
				batchIndex += renderingAreasBatchSize;

				pauseUntilNextBatch = DateTime.Now.AddMilliseconds(1);
			}
		}

		public void Add(VisibilityQueueJob job)
		{
			var currentAreasToGenerate = queue.SelectMany(p => p.AreasToGenerate).Distinct();
			job.AreasToGenerate = job.AreasToGenerate.Except(currentAreasToGenerate).ToList();

			var currentAreasToRender = queue.SelectMany(p => p.AreasToRender).Distinct();
			job.AreasToRender = job.AreasToRender.Except(currentAreasToRender).ToList();

			if (job.AreasToGenerate.Count > 0 ||
				job.AreasToRender.Count > 0)
			{
				queue.Add(job);
			}
		}
	}
}