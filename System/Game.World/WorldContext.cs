using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Physics;
using Game.Entities.System.EntityModel.Handlers;
using Game.World.Terrains.Contexts;
using Microsoft.Xna.Framework;

namespace Game.World
{
	public class WorldContext
	{
		public TerrainContext TerrainContext;
		public IPhysicsHandler PhysicsHandler;
		public IEntityHandler EntityHandler;

		public void Initialize(GameContext context, Vector2i viewDistance, Vector3 gravity, int seed)
		{
			TerrainContext = new TerrainContext(context, viewDistance, seed);
			PhysicsHandler = new JitterPhysicsHandler(gravity);
			EntityHandler = new EntityHandler();
		}
	}
}