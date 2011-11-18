using Framework.Core.Diagnostics.Logging;
using Game.World.Terrains.Generators.Areas;
using Game.World.Terrains.Parts.Areas;

namespace Game.World.Terrains.Generators
{
	/// <summary>
	/// Creates terrain segments (areas) within the world
	/// </summary>
	public partial class TerrainGenerator
	{
		private IAreaGenerator areaGenerator;

		public TerrainGenerator(IAreaGenerator areaGenerator)
		{
			this.areaGenerator = areaGenerator;

			Logger.RegisterLogLevelsFor<TerrainGenerator>(Logger.LogLevels.Adaptive);

			// Start the worker thread
			ThreadingContext.Start(GenerateAsyncLogic);
		}

		/// <summary>
		/// Clears all content the class is using in memory
		/// </summary>
		public void Clear()
		{
			ThreadingContext.Running = false;
		}

		public Area Generate(int x, int y, int z)
		{
			return areaGenerator.Generate(x, y, z);
		}

		public void ReInitialize()
		{
			areaGenerator.ReInitialize();
		}
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