using System;
using System.Text;
using System.Linq;
using Framework.Animations;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Core.Contexts.Input;
using Framework.Core.Scenes;
using Framework.Core.Scenes.Cameras;
using Framework.Core.Services;
using Framework.Gui;
using Game.Entities.Outworld.World;
using Game.Network.Common;
using Game.World.Terrains.Contexts;
using Game.World.Terrains.Helpers;
using Game.World.Terrains.Parts.Areas.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Outworld.Settings.Global;

namespace Outworld.Scenes.Debug.Terrain
{
	public class TerrainDebugScene : SceneBase
	{
		private StringBuilder stringBuilder;
		private TerrainContext terrainContext;
		private Color skyColor;
		private CameraFirstPerson camera;
		private InputContext inputContext;
		private SpatialComponent spatialComponent;
		private Vector2 lookAroundAmplifier;
		private const float radian = (float)Math.PI / 180f;
		private int seed = 140024513;

		private ChatBox chatBox;
		private GuiManager gui;

		private KeyboardState currentKeyboardState;
		private KeyboardState previousKeyboardState;

		float aspectRatio;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			inputContext = context.Input;
			stringBuilder = new StringBuilder(100, 500);
			skyColor = new Color(188, 231, 250, 255);

			lookAroundAmplifier = new Vector2(2f, 2f);

			spatialComponent = new SpatialComponent();
			spatialComponent.Position = new Vector3(18, 80, 80);
			spatialComponent.Angle = new Vector3(180, -20, 0);

			//var globalSettings = ServiceLocator.Get<GlobalSettings>();
			InitializeInput();

			gui = new GuiManager(Context.Input, Context.Graphics.Device, Context.Graphics.SpriteBatch);
		}

		public override void LoadContent()
		{
			var content = Context.Resources.Content;
			var resources = Context.Resources;

			// Terrain
			string[] tiles = { "Grass", "Grass2", "Stone", "Stone2", "Sand", "Mud" };
			for (int i = 1; i <= 6; i++)
			{
				resources.Textures.Add("Tile" + i, content.Load<Texture2D>(@"Tiles\" + tiles[i - 1]));
			}

			terrainContext = new TerrainContext(Context, new Vector2i(5, 3), seed);
			terrainContext.Visibility.Teleport(new Vector3(16.5f, spatialComponent.Position.Y, 16.5f));
			InitializeCamera();

			InitializeGui();

			// Models
			Context.Resources.Models.Add("SkyDome", content.Load<Model>(@"Models\Skies\SkyDome"));

			aspectRatio = Context.Graphics.DeviceManager.GraphicsDevice.Viewport.AspectRatio;
		}

		private void InitializeGui()
		{
			chatBox = new ChatBox(300, 120, 5, Context.Resources.Fonts["Global.Default"]);
			chatBox.HorizontalAlignment = HorizontalAlignment.Right;
			gui.Elements.Add(chatBox);

			gui.UpdateLayout();
		}

		public override void UnloadContent()
		{
			terrainContext.Clear();
			Context.View.Cameras.Clear();

			var resources = Context.Resources;

			for (int i = 1; i <= 6; i++)
			{
				resources.Textures.Remove("Tile" + i);
			}

			// Models
			resources.Models.Remove("SkyDome");
		}

		public override void Update(GameTime gameTime)
		{
			UpdateInput();
			HandleInputMove();
			HandleInputLookAround();
			UpdateCamera(camera);
		}

		public override void Render(GameTime gameTime)
		{
			// Clear the screen
			Context.Graphics.Device.Clear(skyColor);

			// Render the terrain
			terrainContext.Renderer.Render(gameTime, camera);

			// Calculate in which area the camera is at and print it below
			var currentArea = new Vector3i();
			AreaHelper.FindAreaLocation(spatialComponent.Position, ref currentArea);

			stringBuilder.Clear();
			stringBuilder.Append("X: ");
			stringBuilder.Append(spatialComponent.Position.X);
			stringBuilder.Append(", Y: ");
			stringBuilder.Append(spatialComponent.Position.Y);
			stringBuilder.Append(", Z: ");
			stringBuilder.Append(spatialComponent.Position.Z);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("AX: ");
			stringBuilder.Append(spatialComponent.Angle.X);
			stringBuilder.Append(", AY: ");
			stringBuilder.Append(spatialComponent.Angle.Y);
			stringBuilder.Append(", AZ: ");
			stringBuilder.Append(spatialComponent.Angle.Z);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("Area: ");
			stringBuilder.Append(currentArea.X);
			stringBuilder.Append(", ");
			stringBuilder.Append(currentArea.Y);
			stringBuilder.Append(", ");
			stringBuilder.Append(currentArea.Z);

			Context.Graphics.SpriteBatch.Begin();
			Context.Graphics.SpriteBatch.DrawString(Context.Resources.Fonts["Global.Default"],
													stringBuilder,
													new Vector2(3, 0),
													Color.Black,
													0,
													new Vector2(0, 0),
													1,
													SpriteEffects.None,
													0);
			Context.Graphics.SpriteBatch.End();

			//gui.Render();

			RenderSkyDome();
		}


		// Set the position of the model in world space, and set the rotation.
		Vector3 modelPosition = Vector3.Zero;
		float modelRotation = 0.0f;

		private void RenderSkyDome()
		{
			//var camera = Context.View.Cameras["Default"];
			//skinnedModelPlayer.Render(camera.View, camera.Projection, new Vector3(0, 50.0f, 0), 0.0f);

			// Copy any parent transforms.
			Matrix[] transforms = new Matrix[Context.Resources.Models["SkyDome"].Bones.Count];
			Context.Resources.Models["SkyDome"].CopyAbsoluteBoneTransformsTo(transforms);
			
			// Draw the model. A model can have multiple meshes, so loop.
			foreach (ModelMesh mesh in Context.Resources.Models["SkyDome"].Meshes)
			{
				// This is where the mesh orientation is set, as well 
				// as our camera and projection.
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();
					
					effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateRotationY(modelRotation) * Matrix.CreateTranslation(modelPosition);

					effect.View = Matrix.CreateLookAt(camera.Position, Vector3.Zero, Vector3.Up);
					
					effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
				}
				// Draw the mesh, using the effects set above.
				mesh.Draw();
			}
//			base.Draw(gameTime);
		}

		private void InitializeInput()
		{
			Context.Input.Keyboard.ClearMappings();
			Context.Input.Keyboard.AddMapping(Keys.W);
			Context.Input.Keyboard.AddMapping(Keys.A);
			Context.Input.Keyboard.AddMapping(Keys.S);
			Context.Input.Keyboard.AddMapping(Keys.D);
			Context.Input.Keyboard.AddMapping(Keys.E);
			Context.Input.Keyboard.AddMapping(Keys.C);

			Context.Input.Mouse.AutoCenter = true;
			Context.Input.Mouse.ShowCursor = false;

			spatialComponent.Angle.Y = -45;
		}

		private void InitializeCamera()
		{
			camera = new CameraFirstPerson();
			camera.Initialize(Context.Graphics.Device.Viewport);
		}

		public void UpdateCamera(CameraBase camera)
		{
			camera.Position = spatialComponent.Position;

			Matrix cameraRotation = Matrix.CreateRotationX(radian * -spatialComponent.Angle.Y) *
									Matrix.CreateRotationY(radian * -spatialComponent.Angle.X) *
									Matrix.CreateRotationZ(radian * -spatialComponent.Angle.Z);
			Vector3 upVector = Vector3.Transform(Vector3.Up, cameraRotation);

			camera.Target = camera.Position - Vector3.Transform(Vector3.Forward, cameraRotation);
			camera.View = Matrix.CreateLookAt(camera.Position, camera.Target, upVector);
			camera.ApplyToEffect((BasicEffect)Context.Graphics.Effect);
		}

		private void UpdateInput()
		{
			// Get keyboard state
			currentKeyboardState = Keyboard.GetState();
			
			// New seed
			if (currentKeyboardState.IsKeyDown(Keys.F1))
			{
				Random rand = new Random();
				this.seed = rand.Next();
				terrainContext = new TerrainContext(Context, new Vector2i(5, 3), seed);

				terrainContext.Visibility.Teleport(new Vector3(16.5f, spatialComponent.Position.Y, 16.5f));
				
				InitializeCamera();
			}
			// Save terrain-snapshot
			else if (currentKeyboardState.IsKeyDown(Keys.F2))
			{
				ImageExporter.AreasToBitmap(String.Format("d:\\temp\\terrain{0}.png", seed.ToString()), terrainContext.Visibility.AreaCollection.Areas.ToList(), true);
				ImageExporter.AreasToThemeBitmap("d:\\temp\\theme.png", terrainContext.Visibility.AreaCollection.Areas.ToList());
			}
			else if (IsKeyPressedThisUpdate(Keys.T))
			{
				if (!chatBox.IsFocused)
				{
					if (chatBox.Visibility == Visibility.Hidden)
					{
						chatBox.Visibility = Visibility.Visible;
						chatBox.IsFocused = true;
					}
					else if (chatBox.Visibility == Visibility.Visible)
					{
						chatBox.Visibility = Visibility.Hidden;
						chatBox.IsFocused = false;
					}
				}
			}

			gui.UpdateInput();

			previousKeyboardState = currentKeyboardState;
		}

		private bool IsKeyPressedThisUpdate(Keys key)
		{
			if(previousKeyboardState.IsKeyUp(key) && currentKeyboardState.IsKeyDown(key))
			{
				return true;
			}

			return false;
		}

		private void HandleInputMove()
		{
			// Forward / backward
			if (inputContext.Keyboard.KeyboardState[Keys.W].Pressed)
			{
				spatialComponent.Position += GetVelocity(-90);
			}
			else if (inputContext.Keyboard.KeyboardState[Keys.S].Pressed)
			{
				spatialComponent.Position -= GetVelocity(-90);
			}

			// Ascend / Descend
			if (inputContext.Keyboard.KeyboardState[Keys.E].Pressed)
			{
				spatialComponent.Position = new Vector3(spatialComponent.Position.X, spatialComponent.Position.Y + 0.5f, spatialComponent.Position.Z);
			}
			else if (inputContext.Keyboard.KeyboardState[Keys.C].Pressed)
			{
				spatialComponent.Position = new Vector3(spatialComponent.Position.X, spatialComponent.Position.Y - 0.5f, spatialComponent.Position.Z);
			}

			// Strafe left / right
			if (inputContext.Keyboard.KeyboardState[Keys.A].Pressed)
			{
				spatialComponent.Position += GetVelocity(0);
			}
			else if (inputContext.Keyboard.KeyboardState[Keys.D].Pressed)
			{
				spatialComponent.Position -= GetVelocity(0);
			}
		}

		private Vector3 GetVelocity(int angle)
		{
			float velocityX = 0.75f * (float)Math.Cos(radian * (spatialComponent.Angle.X - angle));
			float velocityZ = 0.75f * (float)Math.Sin(radian * (spatialComponent.Angle.X - angle));

			return new Vector3(velocityX, 0, velocityZ);
		}

		private void HandleInputLookAround()
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

				if (spatialComponent.Angle.X < 0)
				{
					spatialComponent.Angle.X += 360f;
				}
			}
			// Look right
			else if (inputContext.Mouse.MouseState[MouseInputType.MoveRight].Pressed)
			{
				spatialComponent.Angle.X += inputContext.Mouse.MouseState[MouseInputType.MoveRight].Value * lookAroundAmplifier.X;

				if (spatialComponent.Angle.X > 359)
				{
					spatialComponent.Angle.X -= 360f;
				}
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
	}
}