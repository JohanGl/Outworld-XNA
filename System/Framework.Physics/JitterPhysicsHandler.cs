using Framework.Physics.RigidBodies;
using Jitter.Collision;
using Microsoft.Xna.Framework;
using Framework.Physics.Renderers;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Physics
{
	public class JitterPhysicsHandler : IPhysicsHandler
	{
		private Jitter.World world;
		public IRigidBodyHandler RigidBodies { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public JitterPhysicsHandler(Vector3 gravity)
		{
			Jitter.Collision.CollisionSystem collision = new CollisionSystemSAP();
			world = new Jitter.World(collision);
			world.AllowDeactivation = true;
			world.Gravity = new Jitter.LinearMath.JVector(gravity.X, gravity.Y, gravity.Z);

			RigidBodies = new JitterRigidBodyHandler(world)
			{
				PhysicsHandler = this
			};
		}

		/// <summary>
		/// Updates the physics state of all objects
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{
			float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (step > 1.0f / 100.0f)
			{
				step = 1.0f / 100.0f;
			}

			world.Step(step, true);

			RigidBodies.Update(gameTime);
		}

		public void Clear()
		{
			world = null;
			RigidBodies.Clear();
			RigidBodies = null;
		}

		public IPhysicsRenderer CreateRenderer(GraphicsDevice device, BasicEffect effect)
		{
			return new JitterPhysicsRenderer(device, effect, world);
		}
	}
}