using System;
using System.Collections.Generic;
using Game.Entities.System;
using Game.Entities.System.ComponentModel;
using Game.Entities.System.EntityModel;
using Microsoft.Xna.Framework;

namespace Outworld.Model.Components.World
{
	/// <summary>
	/// Analyzes a SpatialComponent and provides state information such as if an entity is falling, hitting the ground etc.
	/// </summary>
	public class SpatialSensorComponent : IComponent, IModelUpdateable
	{
		public string Id { get; set; }
		public IEntity Owner { get; set; }

		public Dictionary<SpatialSensorState, bool> State { get; private set; }
		public Vector3 ImpactDepth;

		private SpatialComponent spatialComponent;
		private Vector3 previousVelocity;
		private Vector3 impactDelta;
		private Vector3 previousImpactDelta;
		private Vector3 impactDepth;
		private readonly Vector3 impactTriggerDepth = new Vector3(14, 0.3f, 14);
		private const float AscendDescendTriggerValue = 0.01f;
		private const float HorizontalMovementTriggerValue = 0.5f;
		private const float VerticalMovementTriggerValue = 0.01f;

		public void Initialize()
		{
			State = new Dictionary<SpatialSensorState, bool>();
			State.Add(SpatialSensorState.Ascending, false);
			State.Add(SpatialSensorState.Descending, false);
			State.Add(SpatialSensorState.Impact, false);
			State.Add(SpatialSensorState.HorizontalMovement, false);
			State.Add(SpatialSensorState.VerticalMovement, false);

			impactDelta = new Vector3();
			ImpactDepth = new Vector3();

			spatialComponent = Owner.Components.Get<SpatialComponent>();

			UpdatePreviousVelocity();
		}

		public void Update(GameTime gameTime)
		{
			ResetStates();

			if (IsStationary())
			{
				return;
			}

			UpdateSensorsForAscendDescend();
			UpdateSensorsForHorizontalMovement();
			UpdateSensorsForVerticalMovement();
			UpdateSensorsForImpact();

			UpdatePreviousVelocity();
			UpdatePreviousImpactDelta();
		}

		/// <summary>
		/// Resets all delta values used internally to calculate sensor results.
		/// Handy to call after you´ve manually changed a spatial components position/velocity to avoid triggering impact states.
		/// </summary>
		public void ResetSensorValues()
		{
			UpdatePreviousVelocity();
		}

		private void ResetStates()
		{
			State[SpatialSensorState.Ascending] = false;
			State[SpatialSensorState.Descending] = false;
			State[SpatialSensorState.Impact] = false;
			State[SpatialSensorState.HorizontalMovement] = false;
			State[SpatialSensorState.VerticalMovement] = false;
			ImpactDepth.X = 0;
			ImpactDepth.Y = 0;
			ImpactDepth.Z = 0;
		}

		private bool IsStationary()
		{
			if (spatialComponent.Velocity.X == 0 &&
				spatialComponent.Velocity.Y == 0 &&
				spatialComponent.Velocity.Z == 0 &&
				previousVelocity.X == 0 &&
				previousVelocity.Y == 0 &&
				previousVelocity.Z == 0)
			{
				return true;
			}

			return false;
		}

		private void UpdateSensorsForHorizontalMovement()
		{
			impactDelta.X = Math.Abs(spatialComponent.Velocity.X);
			impactDelta.Z = Math.Abs(spatialComponent.Velocity.Z);

			if (impactDelta.X > HorizontalMovementTriggerValue ||
				impactDelta.Z > HorizontalMovementTriggerValue)
			{
				State[SpatialSensorState.HorizontalMovement] = true;
			}
		}

		private void UpdateSensorsForVerticalMovement()
		{
			if (State[SpatialSensorState.Ascending] || State[SpatialSensorState.Descending])
			{
				State[SpatialSensorState.VerticalMovement] = true;
			}
			else
			{
				impactDelta.Y = Math.Abs(spatialComponent.Velocity.Y);

				if (impactDelta.Y > VerticalMovementTriggerValue)
				{
					State[SpatialSensorState.VerticalMovement] = true;
				}
			}
		}

		private void UpdateSensorsForAscendDescend()
		{
			// Ascending
			if (spatialComponent.Velocity.Y > AscendDescendTriggerValue)
			{
				State[SpatialSensorState.Ascending] = true;
			}
			// Descending
			else if (spatialComponent.Velocity.Y < -AscendDescendTriggerValue)
			{
				State[SpatialSensorState.Descending] = true;
			}
		}

		private void UpdateSensorsForImpact()
		{
			impactDelta.X = Math.Abs(spatialComponent.Velocity.X - previousVelocity.X);
			impactDelta.Y = Math.Abs(spatialComponent.Velocity.Y - previousVelocity.Y);
			impactDelta.Z = Math.Abs(spatialComponent.Velocity.Z - previousVelocity.Z);

			impactDepth.X = impactDelta.X - previousImpactDelta.X;
			impactDepth.Y = impactDelta.Y - previousImpactDelta.Y;
			impactDepth.Z = impactDelta.Z - previousImpactDelta.Z;

			if (impactDepth.X > impactTriggerDepth.X)
			{
				State[SpatialSensorState.Impact] = true;
				ImpactDepth.X = impactDepth.X;
			}

			if (impactDepth.Y > impactTriggerDepth.Y)
			{
				State[SpatialSensorState.Impact] = true;
				ImpactDepth.Y = impactDepth.Y;
			}

			if (impactDepth.Z > impactTriggerDepth.Z)
			{
				State[SpatialSensorState.Impact] = true;
				ImpactDepth.Z = impactDepth.Z;
			}
		}

		private void UpdatePreviousVelocity()
		{
			previousVelocity.X = spatialComponent.Velocity.X;
			previousVelocity.Y = spatialComponent.Velocity.Y;
			previousVelocity.Z = spatialComponent.Velocity.Z;
		}

		private void UpdatePreviousImpactDelta()
		{
			previousImpactDelta.X = impactDelta.X;
			previousImpactDelta.Y = impactDelta.Y;
			previousImpactDelta.Z = impactDelta.Z;
		}
	}
}