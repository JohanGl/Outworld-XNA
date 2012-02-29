using System;
using Framework.Core.Contexts;
using Framework.Core.Contexts.Input;
using Framework.Core.Messaging;
using Framework.Core.Scenes.Cameras;
using Framework.Core.Services;
using Framework.Physics.Controllers;
using Game.Entities;
using Game.Entities.Outworld.World;
using Game.Entities.System;
using Game.Entities.System.ComponentModel;
using Game.Entities.System.EntityModel;
using Game.Network.Clients;
using Game.Network.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Outworld.Settings.Global;

namespace Outworld.Players
{
	public class PlayerInputComponent : IComponent, IModelUpdateable
	{
		public string Id { get; set; }
		public IEntity Owner { get; set; }

		// Custom properties
		public bool IsEnabled { get; set; }
		public bool HasFocus;
		public byte MovementDirection { get; private set; }
		public byte PreviousMovementDirection { get; private set; }
		private IMessageHandler messageHandler;
		private IGameClient gameClient;

		// Components
		private SpatialComponent spatialComponent;
		private PlayerComponent playerComponent;

		private InputContext inputContext;
		private Vector2 lookAroundAmplifier;
		private Vector2 movementAmplifier;
		private float crouchingMovementReduction;
		private const float radian = (float)Math.PI / 180f;
		private CharacterController controller;
		private Vector3 movementVelocity;
		private Vector3 velocityResult;

		public PlayerInputComponent()
		{
			MovementDirection = 0;
			PreviousMovementDirection = 0;
		}

		public void Initialize(InputContext inputContext)
		{
			this.inputContext = inputContext;
			
			playerComponent = Owner.Components.Get<PlayerComponent>();
			spatialComponent = Owner.Components.Get<SpatialComponent>();
			controller = spatialComponent.RigidBody.Tag as CharacterController;

			var globalSettings = ServiceLocator.Get<GlobalSettings>();
			messageHandler = ServiceLocator.Get<IMessageHandler>();
			gameClient = ServiceLocator.Get<IGameClient>();

			// Movement
			lookAroundAmplifier = globalSettings.Player.Movement.LookAroundAmplifier;
			movementAmplifier = globalSettings.Player.Movement.MovementAmplifier;
			crouchingMovementReduction = globalSettings.Player.Movement.CrouchingMovementReduction;

			IsEnabled = true;
		}

		public void Update(GameTime gameTime)
		{
			HandleInput();
		}

		public void UpdateCamera3rdPerson(CameraBase camera)
		{
			camera.Position = spatialComponent.RigidBody.Position;
			camera.Position.Y += playerComponent.CameraOffsetY;

			Vector3 thirdPersonReference = new Vector3(0, 0.4f, 4f);
			Matrix rotationMatrix = Matrix.CreateRotationY(radian * (-spatialComponent.Angle.X + 180));

			// Create a vector pointing the direction the camera is facing.
			Vector3 transformedReference = Vector3.Transform(thirdPersonReference, rotationMatrix);

			// Calculate the position the camera is looking from.
			Vector3 cameraPosition = transformedReference + spatialComponent.Position;

			camera.View = Matrix.CreateLookAt(cameraPosition, spatialComponent.Position, Vector3.Up);
		}
		
		public void UpdateCamera(CameraBase camera)
		{
			camera.Position = spatialComponent.RigidBody.Position;
			camera.Position.Y += playerComponent.CameraOffsetY;

			Matrix cameraRotation = Matrix.CreateRotationX(radian * -spatialComponent.Angle.Y) *
									Matrix.CreateRotationY(radian * -spatialComponent.Angle.X) *
									Matrix.CreateRotationZ(radian * -spatialComponent.Angle.Z);
			Vector3 upVector = Vector3.Transform(Vector3.Up, cameraRotation);
			camera.Target = camera.Position - Vector3.Transform(Vector3.Forward, cameraRotation);

			camera.View = Matrix.CreateLookAt(camera.Position, camera.Target, upVector);
		}

		private void HandleInput()
		{
			velocityResult.X = 0;
			velocityResult.Y = spatialComponent.Velocity.Y;
			velocityResult.Z = 0;

			if (IsEnabled && HasFocus)
			{
				// Handle the different input types
				HandleInputLookAround();
				HandleInputMovement();
				HandleInputStandCrouch();

				controller.TryJump = false;

				// Jump
				if (inputContext.Bindings.Binding[PlayerInputBindings.Jump].WasJustPressed)
				{
					controller.TryJump = true;
					messageHandler.AddMessage(MessageHandlerType.ServerEntityEvents, GetPlayerMessage(EntityEventType.Jump));					
				}
			}

			velocityResult += movementVelocity;

			controller.TargetVelocity = new Jitter.LinearMath.JVector(velocityResult.X, velocityResult.Y, velocityResult.Z);
		}

		private void HandleInputLookAround()
		{
			//// Look up
			//if (inputContext.Bindings.Binding[PlayerInputBindings.LookUp].Pressed)
			//{
			//    spatialComponent.Angle.Y += inputContext.Bindings.Binding[PlayerInputBindings.LookUp].Value * lookAroundAmplifier.Y;
			//}
			//// Look down
			//else if (inputContext.Bindings.Binding[PlayerInputBindings.LookDown].Pressed)
			//{
			//    spatialComponent.Angle.Y -= inputContext.Bindings.Binding[PlayerInputBindings.LookDown].Value * lookAroundAmplifier.Y;
			//}

			if (inputContext.Mouse.MouseState[MouseInputType.MoveUp].Pressed)
			{
				spatialComponent.Angle.Y += inputContext.Mouse.MouseState[MouseInputType.MoveUp].Value * lookAroundAmplifier.Y;
			}
			else if (inputContext.Mouse.MouseState[MouseInputType.MoveDown].Pressed)
			{
				spatialComponent.Angle.Y += inputContext.Mouse.MouseState[MouseInputType.MoveDown].Value * lookAroundAmplifier.Y;
			}

			if (inputContext.Mouse.IsEnabled)
			{
				// Look left
				if (inputContext.Mouse.MouseState[MouseInputType.MoveLeft].Pressed)
				{
					spatialComponent.Angle.X -= inputContext.Mouse.MouseState[MouseInputType.MoveLeft].Value * lookAroundAmplifier.X;
				}
				// Look right
				else if (inputContext.Mouse.MouseState[MouseInputType.MoveRight].Pressed)
				{
					spatialComponent.Angle.X += inputContext.Mouse.MouseState[MouseInputType.MoveRight].Value * lookAroundAmplifier.X;
				}
			}

			//if (inputContext.GamePad.IsEnabled)
			//{
			//    // Look up
			//    if (inputContext.GamePad.GamePadState[Buttons.RightThumbstickUp].Pressed)
			//    {
			//        spatialComponent.Angle.Y += inputContext.GamePad.GamePadState[Buttons.RightThumbstickUp].Value * lookAroundAmplifier.Y;
			//    }
			//        // Look down
			//    else if (inputContext.GamePad.GamePadState[Buttons.RightThumbstickDown].Pressed)
			//    {
			//        spatialComponent.Angle.Y -= inputContext.GamePad.GamePadState[Buttons.RightThumbstickDown].Value * lookAroundAmplifier.Y;
			//    }

			//    // Look left
			//    if (inputContext.GamePad.GamePadState[Buttons.RightThumbstickLeft].Pressed)
			//    {
			//        spatialComponent.Angle.X -= inputContext.GamePad.GamePadState[Buttons.RightThumbstickLeft].Value * lookAroundAmplifier.X;
			//    }
			//        // Look right
			//    else if (inputContext.GamePad.GamePadState[Buttons.RightThumbstickRight].Pressed)
			//    {
			//        spatialComponent.Angle.X += inputContext.GamePad.GamePadState[Buttons.RightThumbstickRight].Value * lookAroundAmplifier.X;
			//    }
			//}

			// Left/right angle limit checks
			if (spatialComponent.Angle.X < 0)
			{
				spatialComponent.Angle.X += 360f;
			}
			else if (spatialComponent.Angle.X >= 360f)
			{
				spatialComponent.Angle.X -= 360f;
			}

			// Up/down angle limit checks
			if (spatialComponent.Angle.Y > 90)
			{
				spatialComponent.Angle.Y = 90;
			}
			else if (spatialComponent.Angle.Y < -90)
			{
				spatialComponent.Angle.Y = -90;
			}
		}

		private void HandleInputMovement()
		{
			movementVelocity.X = 0;
			movementVelocity.Y = 0;
			movementVelocity.Z = 0;

			var forward = inputContext.Bindings.Binding[PlayerInputBindings.Forward];
			var backwards = inputContext.Bindings.Binding[PlayerInputBindings.Backwards];
			var left = inputContext.Bindings.Binding[PlayerInputBindings.Left];
			var right = inputContext.Bindings.Binding[PlayerInputBindings.Right];

			if (forward.Pressed)
			{
				float speed = (forward.Value * movementAmplifier.X);
				movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X + 90));
				movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X + 90));
			}
			else if (backwards.Pressed)
			{
				float speed = (backwards.Value * movementAmplifier.X);
				movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X - 90));
				movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X - 90));
			}

			if (left.Pressed)
			{
				float speed = (left.Value * movementAmplifier.Y);
				movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X));
				movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X));
			}
			else if (right.Pressed)
			{
				float speed = (right.Value * movementAmplifier.Y);
				movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X + 180));
				movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X + 180));
			}

			UpdateInputMovementDirection(forward.Pressed, backwards.Pressed, left.Pressed, right.Pressed);
			
			// Lower the movement speed if crouching
			if (playerComponent.IsCrouching)
			{
				movementVelocity.X *= crouchingMovementReduction;
				movementVelocity.Z *= crouchingMovementReduction;
			}
		}

		private void UpdateInputMovementDirection(bool forward, bool backwards, bool left, bool right)
		{
			PreviousMovementDirection = MovementDirection;

			// Default to idle
			MovementDirection = 0;

			// Here we check what direction the player is moving in an 8 direction clockwise fashion where forward = 1, forward + right = 2, right = 3 etc
			if (forward && left)
			{
				MovementDirection = 8;
			}
			else if (forward && right)
			{
				MovementDirection = 2;
			}
			else if (forward)
			{
				MovementDirection = 1;
			}
			else if (backwards && left)
			{
				MovementDirection = 6;
			}
			else if (backwards && right)
			{
				MovementDirection = 4;
			}
			else if (backwards)
			{
				MovementDirection = 5;
			}
			else if (left)
			{
				MovementDirection = 7;
			}
			else if (right)
			{
				MovementDirection = 3;
			}
		}

		private void HandleInputStandCrouch()
		{
			if (inputContext.Keyboard.IsEnabled)
			{
				if (inputContext.Keyboard.KeyboardState[Keys.LeftShift].WasJustPressed)
				{
					playerComponent.ToggleStandCrouch();
				}
			}

			if (inputContext.GamePad.IsEnabled)
			{
				if (inputContext.GamePad.GamePadState[Buttons.LeftStick].WasJustPressed)
				{
					playerComponent.ToggleStandCrouch();
				}
			}
		}

		private PlayerMessage GetPlayerMessage(EntityEventType type)
		{
			var message = new PlayerMessage();
			message.EntityEvent = new EntityEvent();
			message.EntityEvent.TimeStamp = gameClient.TimeStamp;
			message.EntityEvent.Type = type;

			return message;
		}
	}
}