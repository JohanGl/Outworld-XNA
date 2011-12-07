using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Framework.Animations;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Core.Scenes.Cameras;
using Framework.Core.Services;
using Framework.Physics.Renderers;
using Game.Entities.Outworld;
using Game.Entities.Outworld.Characters;
using Game.Entities.Outworld.World;
using Game.Entities.Outworld.World.SpatialSensor;
using Game.Network.Clients;
using Game.Network.Clients.Events;
using Game.Network.Servers;
using Game.World.Terrains.Helpers;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Areas.Helpers;
using Game.World.Terrains.Parts.Tiles;
using Microsoft.Xna.Framework.Audio;
using Outworld.Players;
using Outworld.Scenes.InGame.Helpers.BreadCrumbs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Outworld.Helpers.EntityFactories;
using Outworld.Scenes.InGame.ChildScenes.InGameMenu;
using Outworld.Settings.Global;

namespace Outworld.Scenes.InGame
{
	public partial class InGameScene : SceneBase
	{
		private IGameServer gameServer;
		private IGameClient gameClient;
		private GlobalSettings globalSettings;
		private IPhysicsRenderer physicsRenderer;
		private StringBuilder stringBuilder;

		private BreadCrumbHelper breadCrumbsHelper;

		/// <summary>
		/// Used for displaying how much memory is being allocated by the application
		/// </summary>
		private Process currentProcess = Process.GetCurrentProcess();

		private Entity player;
		private PlayerComponent playerComponent;
		private PlayerInputComponent playerInput;
		private SpatialComponent playerSpatial;
		private SpatialSensorComponent playerSpatialSensor;
		private HealthComponent playerHealth;
		private Color skyColor;
		private string activeCamera;
		private GameTimer timerSendDataToServer;
		private GameTimer timerWalkingSounds;
		private GameTimer timerSaveBreadCrumb;
		private GameTimer timerUpdateCurrentProcess;

		private SkinnedModel skinnedModelPlayer;

		// Sounds
		private bool walkToggle;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			stringBuilder = new StringBuilder(100, 500);
			
			globalSettings = ServiceLocator.Get<GlobalSettings>();

			// Initialize the server and client
			gameServer = ServiceLocator.Get<IGameServer>();
			gameClient = ServiceLocator.Get<IGameClient>();
			gameClient.GetClientSpatialCompleted += gameClient_GetClientSpatialCompleted;

			physicsRenderer = gameClient.World.PhysicsHandler.CreateRenderer(context.Graphics.Device, (BasicEffect)context.Graphics.Effect);

			InitializeHelpers();
			InitializeWorld();
			InitializeInput();
			InitializeCamera();
			InitializePlayer();
			InitializeTimers();
		}

		private void InitializeHelpers()
		{
			// Allows us to store breadcrumbs for 20 minutes (since breadcrumbs are stored every 10 seconds and we allow 120 entries. 10 * 120 = 1200 seconds which equals 20 minutes)
			breadCrumbsHelper = new BreadCrumbHelper(120);
		}

		private void InitializeWorld()
		{
			skyColor = new Color(188, 231, 250, 255);

			// Initialize the fog
			var effect = Context.Graphics.Effect as BasicEffect;
			effect.FogColor = skyColor.ToVector3();
			effect.FogStart = (globalSettings.World.ViewDistance.X - 1) * (Area.Size.X * Tile.Size.X);
			effect.FogEnd = effect.FogStart + 8;
			effect.FogEnabled = globalSettings.World.Fog;
		}

		private void InitializeCamera()
		{
			var camera = new CameraFirstPerson();
			camera.Initialize(Context.Graphics.Device.Viewport);
			activeCamera = "Default";
			Context.View.Cameras["Default"] = camera;
		}

		private void InitializeInput()
		{
			Context.Input.Keyboard.ClearMappings();
			Context.Input.Keyboard.AddMapping(Keys.Escape);
			Context.Input.Keyboard.AddMapping(Keys.W);
			Context.Input.Keyboard.AddMapping(Keys.A);
			Context.Input.Keyboard.AddMapping(Keys.S);
			Context.Input.Keyboard.AddMapping(Keys.D);
			Context.Input.Keyboard.AddMapping(Keys.LeftShift);
			Context.Input.Keyboard.AddMapping(Keys.Space);
			Context.Input.Keyboard.AddMapping(Keys.Up);
			Context.Input.Keyboard.AddMapping(Keys.Down);
			Context.Input.Keyboard.AddMapping(Keys.Left);
			Context.Input.Keyboard.AddMapping(Keys.Right);
			Context.Input.Keyboard.AddMapping(Keys.F1);
			Context.Input.Keyboard.AddMapping(Keys.F11);
			Context.Input.Keyboard.AddMapping(Keys.F12);

			Context.Input.Mouse.ShowCursor = false;
			Context.Input.Mouse.AutoCenter = true;
		}

		private void InitializePlayer()
		{
			player = PlayerEntityFactory.Get("Player", Context, gameClient.World);

			playerComponent = player.Components.Get<PlayerComponent>();
			playerInput = player.Components.Get<PlayerInputComponent>();
			playerSpatial = player.Components.Get<SpatialComponent>();
			playerSpatialSensor = player.Components.Get<SpatialSensorComponent>();
			playerHealth = player.Components.Get<HealthComponent>();

			// Check if we should spawn at a previous breadcrumb
			if (breadCrumbsHelper.BreadCrumbs.Count > 0)
			{
				var lastBreadCrumb = breadCrumbsHelper.GetLastBreadCrumb();

				playerSpatial.Position = lastBreadCrumb.Position;
				playerSpatial.Angle = lastBreadCrumb.Angle;

				// Remove the last breadcrumb so if we die fast, we respawn from the one before that one, allowing us to step further away from our current position more fluently.
				// This is because we assume that a hostile area isnt preferred to spawn in and try to backtrack to a safer breadcrumb position if possible.
				breadCrumbsHelper.RemoveLastBreadCrumb();
			}

			// Find a suitable spawn point around the player location
			var playerBounds = playerSpatial.GetBoundingBox(globalSettings.Player.Spatial.Size);
			playerSpatial.Position = gameClient.World.TerrainContext.Visibility.SpawnPointHelper.FindSuitableSpawnPoint(playerBounds);

			// Teleport to the spawn point
			gameClient.World.TerrainContext.Visibility.Teleport(playerSpatial.Position);
		}

		private void InitializeTimers()
		{
			timerSendDataToServer = new GameTimer(TimeSpan.FromMilliseconds(1000 / 30), SendDataToServer);
			timerWalkingSounds = new GameTimer(TimeSpan.FromMilliseconds(400), UpdateWalkingSounds);
			timerSaveBreadCrumb = new GameTimer(TimeSpan.FromSeconds(10), SaveBreadCrumb);
			timerUpdateCurrentProcess = new GameTimer(TimeSpan.FromSeconds(2), UpdateCurrentProcess);
		}

		public void Respawn()
		{
			InitializePlayer();
		}

		public override void LoadContent()
		{
			var content = Context.Resources.Content;
			var resources = Context.Resources;

			resources.Fonts.Add("Hud", content.Load<SpriteFont>(@"Fonts\Moire_Numbers"));
			resources.Fonts.Add("Hud.Small", content.Load<SpriteFont>(@"Fonts\Moire_Small"));

			// Terrain
			string[] tiles = { "Grass", "Grass2", "Stone5", "Stone6", "Sand", "Mud" };
			for (int i = 1; i <= 6; i++)
			{
				resources.Textures.Add("Tile" + i, content.Load<Texture2D>(@"Tiles\" + tiles[i - 1]));
			}

			// Gui
			resources.Textures.Add("Gui.Hud.Radar", content.Load<Texture2D>(@"Gui\Scenes\InGame\Radar"));
			resources.Textures.Add("Gui.Hud.ProgressBar", content.Load<Texture2D>(@"Gui\Scenes\InGame\ProgressBar"));
			resources.Textures.Add("Gui.Hud.ProgressBar.Empty", content.Load<Texture2D>(@"Gui\Scenes\InGame\ProgressBar_Empty"));
			resources.Textures.Add("Gui.Hud.WeaponBorder", content.Load<Texture2D>(@"Gui\Scenes\InGame\WeaponBorder"));
			resources.Textures.Add("Gui.Hud.HealthBorder", content.Load<Texture2D>(@"Gui\Scenes\InGame\HealthBorder"));

			InitializeGui();

			// Models
			Context.Resources.Models.Add("Player", content.Load<Model>(@"Models\Characters\Player"));
			Context.Resources.Models.Add("Player2", content.Load<Model>(@"Models\Characters\Dude\dude"));

			skinnedModelPlayer = new SkinnedModel();
			skinnedModelPlayer.Initialize(Context.Resources.Models["Player2"]);
			skinnedModelPlayer.SetAnimationClip("Take 001");
			skinnedModelPlayer.Scale = 0.0225f;

			// Sounds
			Context.Resources.Sounds.Add("Walking1", content.Load<SoundEffect>(@"Sounds\Characters\Walking01"));
			Context.Resources.Sounds.Add("Walking2", content.Load<SoundEffect>(@"Sounds\Characters\Walking02"));
			Context.Resources.Sounds.Add("Landing1", content.Load<SoundEffect>(@"Sounds\Characters\Landing01"));
		}

		public override void UnloadContent()
		{
			gameClient.World.TerrainContext.Clear();
			Context.View.Cameras.Clear();

			var resources = Context.Resources;

			// Textures
			resources.Fonts.Remove("Hud");
			resources.Fonts.Remove("Hud.Small");

			for (int i = 1; i <= 6; i++)
			{
				resources.Textures.Remove("Tile" + i);
			}

			resources.Textures.Remove("Gui.Hud.Radar");
			resources.Textures.Remove("Gui.Hud.ProgressBar");
			resources.Textures.Remove("Gui.Hud.ProgressBar.Empty");
			resources.Textures.Remove("Gui.Hud.WeaponBorder");
			resources.Textures.Remove("Gui.Hud.HealthBorder");

			// Models
			resources.Models.Remove("Player");
			resources.Models.Remove("Player2");

			// Sounds
			Context.Resources.Sounds.Remove("Walking1");
			Context.Resources.Sounds.Remove("Walking2");
			Context.Resources.Sounds.Remove("Landing1");
		}

		public override void Update(GameTime gameTime)
		{
			// Network update
			gameServer.Update(gameTime);
			gameClient.Update(gameTime);

			gameClient.World.TerrainContext.Visibility.Update(ref playerSpatial.RigidBody.Position);

			// Update the player components
			playerInput.HasFocus = HasFocus;
			player.Components.Update(gameTime);

			UpdateCamera();
			UpdateGui(gameTime);

			// Update input
			if (HasFocus)
			{
				UpdateInput();
				timerWalkingSounds.Update(gameTime);
			}

			// Update models
			skinnedModelPlayer.Update(gameTime);

			// Update all timers
			timerSendDataToServer.Update(gameTime);
			timerSaveBreadCrumb.Update(gameTime);
			timerUpdateCurrentProcess.Update(gameTime);
		}

		private void UpdateInput()
		{
			if (Context.Input.Keyboard.KeyboardState[Keys.Escape].WasJustPressed)
			{
				if (SceneChildren.Count == 0)
				{
					Context.Scenes.AddChild(this, new InGameMenuScene(), true);
				}
			}

			if (Context.Input.Keyboard.KeyboardState[Keys.F1].WasJustPressed)
			{
				notifications.AddNotification("Player joined!");
			}

			// Debug tool shortcuts
			if (Context.Input.Keyboard.KeyboardState[Keys.F11].WasJustPressed)
			{
				ImageExporter.AreasToBitmap("snapshot.png", gameClient.World.TerrainContext.Visibility.AreaCollection.Areas.ToList(), true);
				System.Diagnostics.Debug.WriteLine("Snapshot taken");
			}
			
			if (Context.Input.Keyboard.KeyboardState[Keys.F12].WasJustPressed)
			{
				var areaLocation = new Vector3i();
				AreaHelper.FindAreaLocation(playerSpatial.Position, ref areaLocation);

				var area = gameClient.World.TerrainContext.Visibility.AreaCollection.Areas.SingleOrDefault(p => p.Info.Location.ToString() == areaLocation.ToString());

				if (area != null)
				{
					ImageExporter.AreaTo3DBitmap("area3d.png", area);
					System.Diagnostics.Debug.WriteLine("3D Area snapshot taken");
				}
			}
		}

		private void UpdateCamera()
		{
			var camera = Context.View.Cameras[activeCamera];
			playerInput.UpdateCamera(camera);
			//playerInput.UpdateCamera3rdPerson(camera);
			camera.ApplyToEffect((BasicEffect)Context.Graphics.Effect);
		}

		private void UpdateWalkingSounds()
		{
			// Walking sounds
			if (playerSpatialSensor.State[SpatialSensorState.HorizontalMovement] && !playerSpatialSensor.State[SpatialSensorState.VerticalMovement])
			{
				if (!walkToggle)
				{
					Context.Resources.Sounds["Walking1"].Play(0.25f, 0f, -0.1f);
				}
				else
				{
					Context.Resources.Sounds["Walking2"].Play(0.25f, 0f, 0.1f);
				}

				walkToggle = !walkToggle;
			}
			else
			{
				walkToggle = false;
			}

			//// Hit the ground
			//if (!playerSpatialSensor.State[SpatialSensorState.Descending] && previouslyDescending)
			//{
			//    if (gameTime.TotalGameTime.TotalMilliseconds > impactTimer)
			//    {
			//        Context.Resources.Sounds["Landing1"].Play(1.0f, 0f, 0f);
			//        impactTimer = gameTime.TotalGameTime.TotalMilliseconds + 450;
			//    }
			//}

			//previouslyDescending = playerSpatialSensor.State[SpatialSensorState.Descending];
		}

		private void UpdateCurrentProcess()
		{
			currentProcess = Process.GetCurrentProcess();
		}

		public override void Render(GameTime gameTime)
		{
			// Clear the screen
			Context.Graphics.Device.Clear(skyColor);

			// Render the terrain
			gameClient.World.TerrainContext.Renderer.Render(gameTime, Context.View.Cameras[activeCamera]);

			RenderServerEntities();

			RenderGui();
		}

		private void RenderServerEntities()
		{
			//var camera = Context.View.Cameras["Default"];
			//skinnedModelPlayer.Render(camera.View, camera.Projection, playerSpatial.Position + new Vector3(0, -0.725f, 0), playerSpatial.Angle.X + 180f);

			for (int i = 0; i < gameClient.ServerEntities.Count; i++)
			{
				var entity = gameClient.ServerEntities[i];
				RenderModel(entity.Position, entity.Angle.X + 180f);
			}
		}

		private void RenderModel(Vector3 position, float angle)
		{
			var camera = Context.View.Cameras["Default"];
			skinnedModelPlayer.Render(camera.View, camera.Projection, position + new Vector3(0, -0.725f, 0), angle);
		}

		private void SendDataToServer()
		{
			if (gameClient.IsConnected)
			{
				// Sends our current position to the server
				gameClient.SendClientSpatial(playerSpatial.Position, playerSpatial.Velocity, playerSpatial.Angle);
			}
		}

		private void SaveBreadCrumb()
		{
			if (!playerComponent.IsDead)
			{
				breadCrumbsHelper.Add(playerSpatial.Position, playerSpatial.Angle);
			}
		}

		private void gameClient_GetClientSpatialCompleted(object sender, ClientSpatialEventArgs e)
		{
			for (int i = 0; i < e.ClientData.Length; i++)
			{
				var clientData = e.ClientData[i];

				// Forced spatial update from the server
				if (clientData.ClientId == gameClient.ClientId)
				{
					// Move the player to the forced location
				}
				// Update other clients spatial data
				else
				{
					for (int j = 0; j < gameClient.ServerEntities.Count; j++)
					{
						if (gameClient.ServerEntities[j].Id == clientData.ClientId)
						{
							gameClient.ServerEntities[j].Position = clientData.Position;
							gameClient.ServerEntities[j].Velocity = clientData.Velocity;
							gameClient.ServerEntities[j].Angle = clientData.Angle;
							break;
						}
					}

					continue;
				}
			}
		}
	}
}