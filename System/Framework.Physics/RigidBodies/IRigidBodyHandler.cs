using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Framework.Physics.RigidBodies
{
	public interface IRigidBodyHandler
	{
		IPhysicsHandler PhysicsHandler { get; set; }

		void Add(RigidBody rigidBody);
	
		RigidBody Get(string id);
		IEnumerable<RigidBody> GetGroup(string group);

		void Remove(RigidBody rigidBody);
		void RemoveGroup(string group);

		void Update(GameTime gameTime);
		void Clear();

		void SetPosition(RigidBody rigidBody, Vector3 position);
		void SetVelocity(RigidBody rigidBody, Vector3 velocity);
		void ApplyImpulse(RigidBody rigidBody, Vector3 velocity);
		void CanRotate(RigidBody rigidBody, bool state);
	}
}