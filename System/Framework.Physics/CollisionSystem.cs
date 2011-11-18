using System.Collections.Generic;
using Framework.Physics.RigidBodies;
using Jitter.LinearMath;

namespace Framework.Physics
{
	public class CollisionSystem
	{
		private Jitter.World World;
		private Dictionary<RigidBody, Jitter.Dynamics.RigidBody> WorldItems;
		private Jitter.Dynamics.RigidBody Body;

		public CollisionSystem(Jitter.World world, Dictionary<RigidBody, Jitter.Dynamics.RigidBody> worldItems)
		{
			World = world;
			WorldItems = worldItems;
		}

		public bool CanJump(RigidBody body)
		{
			Body = WorldItems[body];

			Jitter.Dynamics.RigidBody resultingBody = null;
			JVector normal;
			float fraction;

			var positions = new JVector[] { new JVector(body.Position.X, body.Position.Y, body.Position.Z),
											new JVector(body.Position.X + 0.5f, body.Position.Y, body.Position.Z),
											new JVector(body.Position.X - 0.5f, body.Position.Y, body.Position.Z),
											new JVector(body.Position.X, body.Position.Y, body.Position.Z - 0.5f),
											new JVector(body.Position.X, body.Position.Y, body.Position.Z + 0.5f)};

			for (int i = 0; i < positions.Length; i++)
			{
				bool result = World.CollisionSystem.Raycast(new JVector(positions[i].X, positions[i].Y, positions[i].Z),
															new JVector(0, -1, 0),
															RaycastCallback,
															out resultingBody,
															out normal,
															out fraction);

				if (result && fraction <= 1.3f && Body.LinearVelocity.Y < 0.5f)
				{
					return true;
				}
			}

			return false;
		}

		//public void RayCast(RigidBody body, Vector3 position, Vector3 direction)
		//{
		//    Body = WorldItems[body];

		//    Jitter.Dynamics.RigidBody resultingBody = null;
		//    JVector normal;
		//    float fraction;
		//    float JumpVelocity = 0.5f;

		//    bool result = World.CollisionSystem.Raycast(new JVector(position.X, position.Y, position.Z),
		//                                                new JVector(direction.X, direction.Y, direction.Z),
		//                                                RaycastCallback,
		//                                                out resultingBody,
		//                                                out normal,
		//                                                out fraction);

		//    var BodyWalkingOn = (result && fraction <= 0.2f) ? resultingBody : null;
		//    bool canJump = (result && fraction <= 0.2f && Body.LinearVelocity.Y < JumpVelocity);
		//}

		private bool RaycastCallback(Jitter.Dynamics.RigidBody body, JVector normal, float fraction)
		{
			// prevent the ray to collide with ourself!
			return (body != Body);
		}
	}
}