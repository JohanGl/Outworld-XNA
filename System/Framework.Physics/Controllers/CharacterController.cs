using System.Collections.Generic;
using Jitter.LinearMath;

namespace Framework.Physics.Controllers
{
	public class CharacterController : Jitter.Dynamics.Constraints.Constraint
	{
		private const float JumpVelocity = 0.8f;
		private float feetPosition;
		public Jitter.World World { private set; get; }
		public JVector TargetVelocity { get; set; }
		public bool TryJump { get; set; }
		private JVector deltaVelocity = JVector.Zero;
		private bool shouldIJump;

		public CharacterController(Jitter.World world, Jitter.Dynamics.RigidBody body) : base(body, null)
		{
			this.World = world;

			// determine the position of the feets of the character
			// this can be done by supportmapping in the down direction.
			// (furthest point away in the down direction)
			JVector vec = JVector.Down;
			JVector result = JVector.Zero;

			// Note: the following works just for normal shapes, for multishapes (compound for example)
			// you have to loop through all sub-shapes -> easy.
			body.Shape.SupportMapping(ref vec, out result);

			// feet position is now the distance between body.Position and the feets
			// Note: the following '*' is the dot product.
			feetPosition = result * JVector.Down;
		}

		public override void AddToDebugDrawList(List<JVector> lineList, List<JVector> pointList)
		{
			// nothing to debug draw
		}

		public override void PrepareForIteration(float timestep)
		{
			// send a ray from our feet position down.
			// if we collide with something which is 0.05f units below our feets remember this!

			Jitter.Dynamics.RigidBody resultingBody = null;
			JVector normal;
			float fraction;

			bool result = World.CollisionSystem.Raycast(Body1.Position + JVector.Down * (feetPosition - 0.1f), JVector.Down, RaycastCallback, out resultingBody, out normal, out fraction);

			shouldIJump = TryJump;
			//shouldIJump = (result && fraction <= 0.2f && Body1.LinearVelocity.Y < JumpVelocity && TryJump);
		}

		private bool RaycastCallback(Jitter.Dynamics.RigidBody body, JVector normal, float fraction)
		{
			// prevent the ray to collide with ourself!
			return (body != this.Body1);
		}

		public override void Iterate()
		{
			deltaVelocity = TargetVelocity - Body1.LinearVelocity;
			deltaVelocity.Y = 0.0f;

			// determine how 'stiff' the character follows the target velocity
			deltaVelocity *= 0.02f;

			if (deltaVelocity.LengthSquared() != 0.0f)
			{
				// activate it, in case it fall asleep :)
				Body1.IsActive = true;
				Body1.ApplyImpulse(deltaVelocity * Body1.Mass);
			}

			if (shouldIJump)
			{
				Body1.IsActive = true;
				Body1.ApplyImpulse(JumpVelocity * JVector.Up * Body1.Mass);
			}
		}
	}
}