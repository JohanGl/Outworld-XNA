using Framework.Core.Common;
using Framework.Core.Contexts;
using Game.World.Terrains.Parts.Areas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Outworld.Settings.Global
{
	public class GlobalSettings
	{
		public WorldSettings World { get; set; }
		public PlayerSettings Player { get; set; }
		public InputSettings Input { get; set; }
		public NetworkSettings Network { get; set; }
		public AudioSettings Audio { get; set; }

		public GlobalSettings(bool initializeDefaults = true)
		{
			World = new WorldSettings();
			Player = new PlayerSettings();
			Input = new InputSettings();
			Network = new NetworkSettings();
			Audio = new AudioSettings();

			if (initializeDefaults)
			{
				InitializeDefaults();
			}
		}

		public void InitializeDefaults()
		{
			// World (seed 500 = buggy)
			World.Seed = 140024513;
			World.Fog = true;
			World.Gravity = new Vector3(0, -30, 0);
			//World.ViewDistance = new Vector2i(3, 3);
			World.ViewDistance = new Vector2i(3, 3);

			// Player
			Player.CameraOffsetY = 0.7f;
			Player.CameraCrouchingOffsetY = -0.1f;
			Player.ImpactDamageAmplifier = 75f;
			Player.Health = 100;
			Player.MaxHealth = 100;

			Player.Spatial.Position = new Vector3(Area.HalfSize.X + 0.5f, Area.Size.Y, Area.HalfSize.Z + 0.5f);
			Player.Spatial.Angle = new Vector3(180, 0, 0);
			Player.Spatial.Velocity = new Vector3(0, 0, 0);
			Player.Spatial.Area = new Vector3i(0, 0, 0);
			Player.Spatial.Size = new Vector3(0.3f, 1.9f, 0.3f);
			Player.Spatial.CollisionDetectionBounds = new Vector3(4, 6, 4);

			Player.Movement.LookAroundAmplifier = new Vector2(2f, 2f);
			Player.Movement.MovementAmplifier = new Vector2(14f, 10f);
			Player.Movement.CrouchingMovementReduction = 0.5f;

			Player.AnimationDuration.Stand = 200;
			Player.AnimationDuration.Crouch = 200;
			Player.AnimationDuration.Death = 450;

			// Input
			Input.GamePad.IsRumbleActivated = true;

			Input.Keyboard.Mappings.Clear();
			Input.Keyboard.Mappings.Add(Keys.W, Buttons.LeftThumbstickUp);
			Input.Keyboard.Mappings.Add(Keys.A, Buttons.LeftThumbstickLeft);
			Input.Keyboard.Mappings.Add(Keys.S, Buttons.LeftThumbstickDown);
			Input.Keyboard.Mappings.Add(Keys.D, Buttons.LeftThumbstickRight);
			Input.Keyboard.Mappings.Add(Keys.Space, Buttons.A);
			Input.Keyboard.Mappings.Add(Keys.LeftShift, Buttons.LeftStick);
			Input.Keyboard.Mappings.Add(Keys.Escape, Buttons.Back);
			Input.Keyboard.Mappings.Add(Keys.Up, Buttons.DPadUp);
			Input.Keyboard.Mappings.Add(Keys.Down, Buttons.DPadDown);

			Input.Mouse.Mappings.Clear();
			Input.Mouse.Mappings.Add(MouseInputType.MoveUp, Buttons.RightThumbstickUp);
			Input.Mouse.Mappings.Add(MouseInputType.MoveDown, Buttons.RightThumbstickDown);
			Input.Mouse.Mappings.Add(MouseInputType.MoveLeft, Buttons.RightThumbstickLeft);
			Input.Mouse.Mappings.Add(MouseInputType.MoveRight, Buttons.RightThumbstickRight);
			Input.Mouse.Mappings.Add(MouseInputType.LeftButton, Buttons.RightTrigger);
			Input.Mouse.Mappings.Add(MouseInputType.RightButton, Buttons.A);

			// Network
			Network.ServerAddress = "127.0.0.1";
			Network.ServerPort = 14242;
			Network.ClientPort = 14242;
			Network.MaximumConnections = 4;

			// Audio
			Audio.MusicVolume = 1f;
			Audio.SoundVolume = 1f;
		}
	}
}