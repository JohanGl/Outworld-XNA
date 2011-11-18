using System.Collections.Generic;
using Framework.Core.Common;
using Game.World.Terrains.Parts.Areas;

namespace Game.World.Terrains.Visibility.Queues
{
	public class VisibilityQueueJob
	{
		/// <summary>
		/// Defines the range of which the job affects. Used to double check that we dont have 2 jobs of the same range scheduled
		/// </summary>
		public AreaRange Range { get; private set; }

		public VisibilityQueueJobStatus Status;

		public List<Vector3i> AreasToGenerate;
		public List<Area> AreasToRender;
		public List<Area> AreasToRenderNeighbors;

		public VisibilityQueueJob(AreaRange range)
		{
			Range = range;
			AreasToGenerate = new List<Vector3i>();
			AreasToRender = new List<Area>();
			AreasToRenderNeighbors = new List<Area>();
		}
	}

	public enum VisibilityQueueJobStatus
	{
		Ready,
		GeneratingAreas,
		GeneratingAreasCompleted,
		RenderingAreas
	}
}