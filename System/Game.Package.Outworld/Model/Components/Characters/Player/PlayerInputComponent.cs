using System;
using Framework.Core.Contexts;
using Framework.Core.Scenes.Cameras;
using Framework.Core.Services;
using Framework.Physics.Controllers;
using Game.Model;
using Game.Model.Components;
using Game.Model.Entities;
using Outworld.Model.Components.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Outworld.Settings.Global;

namespace Outworld.Model.Components.Characters.Player
{
	public class PlayerInputComponent : IComponent, IModelUpdateable
	{
		public string Id { get; set; }
		public IEntity Owner { get; set; }

		// Custom properties
		public bool IsEnabled { get; set; }
		public bool HasFocus;

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

		public void Initialize(InputContext inputContext)
		{
			this.inputContext = inputContext;
			playerComponent = Owner.Components.Get<PlayerComponent>();
			spatialComponent = Owner.Components.Get<SpatialComponent>();
			controller = spatialComponent.RigidBody.Tag as CharacterController;

			var globalSettings = ServiceLocator.Get<GlobalSettings>();

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
				if (inputContext.HasGamePad)
				{
					if (inputContext.GamePadState[Buttons.A].WasJustPressed)
					{
						controller.TryJump = true;
					}
				}

				if (inputContext.Keyboard.IsEnabled)
				{
					if (inputContext.Keyboard.KeyboardState[Keys.Space].WasJustPressed)
					{
						controller.TryJump = true;
					}
				}
			}

			velocityResult += movementVelocity;

			controller.TargetVelocity = new Jitter.LinearMath.JVector(velocityResult.X, velocityResult.Y, velocityResult.Z);
		}

		private void HandleInputLookAround()
		{
			if (inputContext.Mouse.IsEnabled)
			{
				// Look up
				if (inputContext.Mouse.MouseState[MouseInputType.MoveUp].Pressed)
				{
					spatialComponent.Angle.Y += inputContext.Mouse.MouseState[MouseInputType.MoveUp].Value * lookAroundAmplifier.Y;
				}
				// Look down
				else if (inputContext.Mouse.MouseState[MouseInputType.MoveDown].Pressed)
				{
					spatialComponent.Angle.Y -= inputContext.Mouse.MouseState[MouseInputType.MoveDown].Value * lookAroundAmplifier.Y;
				}

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
			
			if (inputContext.HasGamePad)
			{
				// Look up
				if (inputContext.GamePadState[Buttons.RightThumbstickUp].Pressed)
				{
					spatialComponent.Angle.Y += inputContext.GamePadState[Buttons.RightThumbstickUp].Value * lookAroundAmplifier.Y;
				}
					// Look down
				else if (inputContext.GamePadState[Buttons.RightThumbstickDown].Pressed)
				{
					spatialComponent.Angle.Y -= inputContext.GamePadState[Buttons.RightThumbstickDown].Value * lookAroundAmplifier.Y;
				}

				// Look left
				if (inputContext.GamePadState[Buttons.RightThumbstickLeft].Pressed)
				{
					spatialComponent.Angle.X -= inputContext.GamePadState[Buttons.RightThumbstickLeft].Value * lookAroundAmplifier.X;
				}
					// Look right
				else if (inputContext.GamePadState[Buttons.RightThumbstickRight].Pressed)
				{
					spatialComponent.Angle.X += inputContext.GamePadState[Buttons.RightThumbstickRight].Value * lookAroundAmplifier.X;
				}
			}

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

			if (inputContext.Keyboard.IsEnabled)
			{
				// Walk forward
				if (inputContext.Keyboard.KeyboardState[Keys.W].Pressed)
				{
					float speed = (1f * movementAmplifier.X);
					movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X + 90));
					movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X + 90));
				}
				// Walk backwards
				else if (inputContext.Keyboard.KeyboardState[Keys.S].Pressed)
				{
					float speed = (1f * movementAmplifier.X);
					movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X - 90));
					movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X - 90));
				}

				// Strafe left
				if (inputContext.Keyboard.KeyboardState[Keys.A].Pressed)
				{
					float speed = (1f * movementAmplifier.Y);
					movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X));
					movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X));
				}
				// Strafe right
				else if (inputContext.Keyboard.KeyboardState[Keys.D].Pressed)
				{
					float speed = (1f * movementAmplifier.Y);
					movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X + 180));
					movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X + 180));
				}
			}
			
			if (inputContext.HasGamePad)
			{
				// Walk forward
				if (inputContext.GamePadState[Buttons.LeftThumbstickUp].Pressed)
				{
					float speed = (inputContext.GamePadState[Buttons.LeftThumbstickUp].Value * movementAmplifier.X);
					movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X + 90));
					movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X + 90));
				}
					// Walk backwards
				else if (inputContext.GamePadState[Buttons.LeftThumbstickDown].Pressed)
				{
					float speed = (inputContext.GamePadState[Buttons.LeftThumbstickDown].Value * movementAmplifier.X);
					movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X - 90));
					movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X - 90));
				}

				// Strafe left
				if (inputContext.GamePadState[Buttons.LeftThumbstickLeft].Pressed)
				{
					float speed = (inputContext.GamePadState[Buttons.LeftThumbstickLeft].Value * movementAmplifier.Y);
					movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X));
					movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X));
				}
					// Strafe right
				else if (inputContext.GamePadState[Buttons.LeftThumbstickRight].Pressed)
				{
					float speed = (inputContext.GamePadState[Buttons.LeftThumbstickRight].Value * movementAmplifier.Y);
					movementVelocity.X += speed * (float)Math.Cos(radian * (spatialComponent.Angle.X + 180));
					movementVelocity.Z += speed * (float)Math.Sin(radian * (spatialComponent.Angle.X + 180));
				}
			}

			// Lower the movement speed if crouching
			if (playerComponent.IsCrouching)
			{
				movementVelocity.X *= crouchingMovementReduction;
				movementVelocity.Z *= crouchingMovementReduction;
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

			if (inputContext.HasGamePad)
			{
				if (inputContext.GamePadState[Buttons.LeftStick].WasJustPressed)
				{
					playerComponent.ToggleStandCrouch();
				}
			}
		}
	}
}