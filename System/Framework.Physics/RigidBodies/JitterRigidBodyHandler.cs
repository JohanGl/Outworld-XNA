using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Physics.Controllers;
using Framework.Physics.RigidBodies.Shapes;
using Jitter.LinearMath;
using Microsoft.Xna.Framework;

namespace Framework.Physics.RigidBodies
{
	public class JitterRigidBodyHandler : IRigidBodyHandler
	{
		private Jitter.World world;
		private Dictionary<string, RigidBody> items;
		private Dictionary<RigidBody, Jitter.Dynamics.RigidBody> worldItems;

		public IPhysicsHandler PhysicsHandler { get; set; }

		public JitterRigidBodyHandler(Jitter.World world)
		{
			this.world = world;
			items = new Dictionary<string, RigidBody>();
			worldItems = new Dictionary<RigidBody, Jitter.Dynamics.RigidBody>();
		}

		public void Add(RigidBody rigidBody)
		{
			if (items.ContainsKey(rigidBody.Id))
			{
				Remove(items[rigidBody.Id]);
			}

			items.Add(rigidBody.Id, rigidBody);

			AddRigidBodyToWorld(rigidBody);
		}

		public RigidBody Get(string id)
		{
			if (items.ContainsKey(id))
			{
				return items[id];
			}

			return null;
		}

		public IEnumerable<RigidBody> GetGroup(string group)
		{
			var result = new List<RigidBody>();

			foreach (var item in items)
			{
				if (item.Value.Group == group)
				{
					result.Add(item.Value);
				}
			}

			return result;
		}

		public void Remove(RigidBody rigidBody)
		{
			// Remove the item from the world
			world.RemoveBody(worldItems[rigidBody]);
			worldItems.Remove(rigidBody);

			// Remove the item from the items
			items.Remove(rigidBody.Id);
		}

		public void RemoveGroup(string group)
		{
			var items = GetGroup(group).ToList();
			items.ForEach(p => Remove(p));
		}

		public void Update(GameTime gameTime)
		{
			foreach (var pair in worldItems)
			{
				var item = pair.Key;
				var body = pair.Value;

				item.Velocity.X = body.LinearVelocity.X;
				item.Velocity.Y = body.Position.Y - item.Position.Y;	// Jitter doesnt provide a very accurate Y velocity so we calculate it ourselves like this
				item.Velocity.Z = body.LinearVelocity.Z;

				item.Position.X = body.Position.X;
				item.Position.Y = body.Position.Y;
				item.Position.Z = body.Position.Z;

				item.Orientation.M11 = body.Orientation.M11;
				item.Orientation.M12 = body.Orientation.M12;
				item.Orientation.M13 = body.Orientation.M13;
				item.Orientation.M14 = 0f;
				item.Orientation.M21 = body.Orientation.M21;
				item.Orientation.M22 = body.Orientation.M22;
				item.Orientation.M23 = body.Orientation.M23;
				item.Orientation.M24 = 0f;
				item.Orientation.M31 = body.Orientation.M31;
				item.Orientation.M32 = body.Orientation.M32;
				item.Orientation.M33 = body.Orientation.M33;
				item.Orientation.M34 = 0f;
				item.Orientation.M41 = 0f;
				item.Orientation.M42 = 0f;
				item.Orientation.M43 = 0f;
				item.Orientation.M44 = 0f;
			}
		}

		public void Clear()
		{
			items.Clear();
			worldItems.Clear();
			world.Clear();
		}

		public void SetPosition(RigidBody rigidBody, Vector3 position)
		{
			rigidBody.Position = position;
			worldItems[rigidBody].Position = new JVector(position.X, position.Y, position.Z);
		}

		public void SetVelocity(RigidBody rigidBody, Vector3 velocity)
		{
			rigidBody.Velocity = velocity;
			worldItems[rigidBody].LinearVelocity = new JVector(velocity.X, velocity.Y, velocity.Z);
		}

		public void ApplyImpulse(RigidBody rigidBody, Vector3 velocity)
		{
			worldItems[rigidBody].ApplyImpulse(new JVector(velocity.X, velocity.Y, velocity.Z));
		}

		public void CanRotate(RigidBody rigidBody, bool state)
		{
			worldItems[rigidBody].UseUserMassProperties(JMatrix.Zero, 1.0f, true);
			worldItems[rigidBody].Restitution = 0.0f;
		}

		private void AddRigidBodyToWorld(RigidBody item)
		{
			// Initialize the shape
			Jitter.Collision.Shapes.Shape shape = null;

			// Box shape
			if (item.Shape is BoxShape)
			{
				var shapeInfo = item.Shape as BoxShape;
				shape = new Jitter.Collision.Shapes.BoxShape(shapeInfo.Size.X, shapeInfo.Size.Y, shapeInfo.Size.Z);
			}
			// Capsule shape
			else if (item.Shape is CapsuleShape)
			{
				var shapeInfo = item.Shape as CapsuleShape;
				shape = new Jitter.Collision.Shapes.CapsuleShape(shapeInfo.Length, shapeInfo.Radius);
			}
			// Default shape
			else
			{
				throw new NotImplementedException("The specified shape has not been implemented");
			}

			// Initialize the body with the specified shape
			var body = new Jitter.Dynamics.RigidBody(shape)
			{
				Position = new JVector(item.Position.X, item.Position.Y, item.Position.Z),
				IsStatic = item.IsStatic,
				DynamicFriction = 0,
				Mass = item.Mass
			};

			// Add the body to the world
			world.AddBody(body);

			// Map the item to the world item
			worldItems.Add(item, body);

			// TODO: DEBUG code only
			if (item.Id == "Player")
			{
				var constraint = new CharacterController(world, body);
				world.AddConstraint(constraint);
				item.Tag = constraint;
			}
		}
	}
}