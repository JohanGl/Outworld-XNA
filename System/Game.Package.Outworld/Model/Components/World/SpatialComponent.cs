using Framework.Core.Common;
using Framework.Physics.RigidBodies;
using Game.Model;
using Game.Model.Components;
using Game.Model.Entities;
using Microsoft.Xna.Framework;

namespace Outworld.Model.Components.World
{
	public class SpatialComponent : IComponent
	{
		public string Id { get; set; }
		public IEntity Owner { get; set; }

		private Vector3 position;
		public Vector3 Position
		{
			get
			{
				if (RigidBody != null)
				{
					return RigidBody.Position;
				}

				return position;
			}

			set
			{
				position = value;
				
				if (RigidBody != null)
				{
					RigidBody.RigidBodyHandler.SetPosition(RigidBody, position);
				}
			}
		}

		private Vector3 velocity;
		public Vector3 Velocity
		{
			get
			{
				if (RigidBody != null)
				{
					return RigidBody.Velocity;
				}

				return velocity;
			}

			set
			{
				velocity = value;

				if (RigidBody != null)
				{
					RigidBody.RigidBodyHandler.SetVelocity(RigidBody, velocity);
				}
			}
		}

		public Vector3 Angle;
		public Vector3i Area;
		public RigidBody RigidBody;

		/// <summary>
		/// The extended bounds of the entity used to detect nearby entities
		/// </summary>
		public Vector3 CollisionDetectionBounds;

		/// <summary>
		/// Calculates the bounding box world coordinates based on the component position and a defined size
		/// </summary>
		/// <param name="size">The total length of the bounding box on all axes</param>
		/// <param name="expandUpOnAxisY">Expands the bounding box upwards where the Y axis of the position is the bottom of the bounding box. Otherwise the center of all axes are the center of the bounding box</param>
		public BoundingBox GetBoundingBox(Vector3 size, bool expandUpOnAxisY = false)
		{
			size /= 2;

			float minY = expandUpOnAxisY ? 0 : size.Y;
			float maxY = expandUpOnAxisY ? size.Y * 2 : size.Y;

			var result = new BoundingBox
			{
				Min = Position - new Vector3(size.X, minY, size.Z),
				Max = Position + new Vector3(size.X, maxY, size.Z)
			};

			return result;
		}
	}
}