using Framework.Physics.Renderers;
using Framework.Physics.RigidBodies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Physics
{
	public interface IPhysicsHandler
	{
		IRigidBodyHandler RigidBodies { get; set; }
		void Update(GameTime gameTime);
		void Clear();
		IPhysicsRenderer CreateRenderer(GraphicsDevice device, BasicEffect effect);
	}
}