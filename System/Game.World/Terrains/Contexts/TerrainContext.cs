using Framework.Core.Common;
using Framework.Core.Contexts;
using Game.World.Terrains.Generators;
using Game.World.Terrains.Generators.Areas;
using Game.World.Terrains.Helpers;
using Game.World.Terrains.Parts.Areas.AreaModels;
using Game.World.Terrains.Rendering;
using Game.World.Terrains.Rendering.MeshPools;
using Game.World.Terrains.Visibility;

namespace Game.World.Terrains.Contexts
{
	public class TerrainContext
	{
		public TerrainGenerator Generator;
		public TerrainRenderer Renderer;
		public TerrainVisibility Visibility;
		public TerrainContextCollisionHelper TerrainContextCollisionHelper;
		public TileCollisionHelper TileCollisionHelper;

		public TerrainContext(GameContext context, Vector2i viewDistance, int seed)
		{
			Generator = new TerrainGenerator(new AreaGeneratorAdvanced(seed));
			Renderer = new TerrainRenderer();
			Visibility = new TerrainVisibility();
			TerrainContextCollisionHelper = new TerrainContextCollisionHelper(this);
			TileCollisionHelper = new TileCollisionHelper();

			Renderer.Initialize(Visibility, context);
			Visibility.Initialize(viewDistance, this);

			TerrainMeshPool.Initialize(context.Graphics.Device);
			AreaModelHandler.Initialize(Visibility, context);
		}

		/// <summary>
		/// Clears all content within the context
		/// </summary>
		public void Clear()
		{
			Generator.Clear();
			Renderer.Clear();
			Visibility.Clear();
			TerrainMeshPool.Clear();
		}
	}
}