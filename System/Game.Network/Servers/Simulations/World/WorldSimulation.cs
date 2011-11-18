using Framework.Physics;
using Game.Entities.System.EntityModel.Handlers;
using Game.Network.Servers.Simulations.World.Terrains;
using Microsoft.Xna.Framework;

namespace Game.Network.Servers.Simulations.World
{
	public class WorldSimulation
	{
		public TerrainContext TerrainContext;
		public IPhysicsHandler PhysicsHandler;
		public IEntityHandler EntityHandler;

		public void Initialize(Vector3 gravity, int seed)
		{
			TerrainContext = new TerrainContext(seed);
			PhysicsHandler = new JitterPhysicsHandler(gravity);
			EntityHandler = new EntityHandler();
		}

		public void Update(GameTime gameTime)
		{
			PhysicsHandler.Update(gameTime);
			EntityHandler.Update(gameTime);
		}
	}
}