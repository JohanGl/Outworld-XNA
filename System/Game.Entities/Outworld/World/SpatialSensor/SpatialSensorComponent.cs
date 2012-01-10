using System;
using System.Collections.Generic;
using Game.Entities.System;
using Game.Entities.System.ComponentModel;
using Game.Entities.System.EntityModel;
using Microsoft.Xna.Framework;

namespace Game.Entities.Outworld.World.SpatialSensor
{
	/// <summary>
	/// Analyzes a SpatialComponent and provides state information such as if an entity is falling, hitting the ground etc.
	/// </summary>
	public class SpatialSensorComponent : IComponent, IModelUpdateable
	{
		public string Id { get; set; }
		public IEntity Owner { get; set; }

		public Dictionary<SpatialSensorStateType, SpatialSensorState> State { get; private set; }

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
			State = new Dictionary<SpatialSensorStateType, SpatialSensorState>();
			State.Add(SpatialSensorStateType.Ascending, new SpatialSensorState());
			State.Add(SpatialSensorStateType.Descending, new SpatialSensorState());
			State.Add(SpatialSensorStateType.Impact, new SpatialSensorState());
			State.Add(SpatialSensorStateType.HorizontalMovement, new SpatialSensorState());
			State.Add(SpatialSensorStateType.VerticalMovement, new SpatialSensorState());

			impactDelta = new Vector3();

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
			State[SpatialSensorStateType.Ascending].IsActive = false;
			State[SpatialSensorStateType.Descending].IsActive = false;
			State[SpatialSensorStateType.Impact].IsActive = false;
			State[SpatialSensorStateType.HorizontalMovement].IsActive = false;
			State[SpatialSensorStateType.VerticalMovement].IsActive = false;

			State[SpatialSensorStateType.Impact].Value = Vector3.Zero;
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
				State[SpatialSensorStateType.HorizontalMovement].IsActive = true;
			}
		}

		private void UpdateSensorsForVerticalMovement()
		{
			if (State[SpatialSensorStateType.Ascending].IsActive || State[SpatialSensorStateType.Descending].IsActive)
			{
				State[SpatialSensorStateType.VerticalMovement].IsActive = true;
			}
			else
			{
				impactDelta.Y = Math.Abs(spatialComponent.Velocity.Y);

				if (impactDelta.Y > VerticalMovementTriggerValue)
				{
					State[SpatialSensorStateType.VerticalMovement].IsActive = true;
				}
			}
		}

		private void UpdateSensorsForAscendDescend()
		{
			// Ascending
			if (spatialComponent.Velocity.Y > AscendDescendTriggerValue)
			{
				State[SpatialSensorStateType.Ascending].IsActive = true;
			}
			// Descending
			else if (spatialComponent.Velocity.Y < -AscendDescendTriggerValue)
			{
				State[SpatialSensorStateType.Descending].IsActive = true;
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
				State[SpatialSensorStateType.Impact].IsActive = true;
				State[SpatialSensorStateType.Impact].Value.X = impactDepth.X;
			}

			if (impactDepth.Y > impactTriggerDepth.Y)
			{
				State[SpatialSensorStateType.Impact].IsActive = true;
				State[SpatialSensorStateType.Impact].Value.Y = impactDepth.Y;
			}

			if (impactDepth.Z > impactTriggerDepth.Z)
			{
				State[SpatialSensorStateType.Impact].IsActive = true;
				State[SpatialSensorStateType.Impact].Value.Z = impactDepth.Z;
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