using System.Text;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Game.Network.Clients;
using Game.Network.Servers;
using Game.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Outworld.Settings.Global;

namespace Outworld.Scenes.InGame
{
	public class LoadingScene : SceneBase
	{
		private IGameServer gameServer;
		private IGameClient gameClient;
		private StringBuilder stringBuilder;
		private bool startedLoading;
		private bool hasRenderedContent;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			stringBuilder = new StringBuilder(100, 500);

			gameServer = ServiceLocator.Get<IGameServer>();
			gameClient = ServiceLocator.Get<IGameClient>();

			// Create the connection between the client and server
			gameServer.Start();
			gameClient.Connect();

			startedLoading = false;
			hasRenderedContent = false;
		}

		public override void LoadContent()
		{
		}

		public override void UnloadContent()
		{
		}

		public override void Update(GameTime gameTime)
		{
			// Network update
			gameServer.Update(gameTime);
			gameClient.Update(gameTime);

			if (!gameClient.IsConnected || !hasRenderedContent)
			{
				return;
			}

			if (!startedLoading)
			{
				startedLoading = true;
				GetGameSettings();
			}
		}

		public override void Render(GameTime gameTime)
		{
			stringBuilder.Clear(); 
			stringBuilder.Append("Loading");

			Context.Graphics.Device.Clear(Color.Black);

			Context.Graphics.SpriteBatch.Begin();

			Context.Graphics.SpriteBatch.DrawString(Context.Resources.Fonts["Global.Default"],
													stringBuilder,
													new Vector2(3, 0),
													Color.White,
													0,
													new Vector2(0, 0),
													1,
													SpriteEffects.None,
													0);
			Context.Graphics.SpriteBatch.End();

			hasRenderedContent = true;
		}

		private void GetGameSettings()
		{
			gameClient.GetGameSettingsCompleted += (s, e) => InitializeGameSettings(e);
			gameClient.GetGameSettings();
		}

		private void InitializeGameSettings(GameSettingsEventArgs e)
		{
			// Initialize the settings
			var settings = ServiceLocator.Get<GlobalSettings>();
			settings.World.Seed = e.Seed;
			settings.World.Gravity = e.Gravity;

			// Initialize the world
			gameClient.World = new WorldContext();
			gameClient.World.Initialize(Context, settings.World.ViewDistance, settings.World.Gravity, settings.World.Seed);

			ExitScene();
		}

		private void ExitScene()
		{
			// Before exiting the current scene, initialize the target scene
			var targetScene = new InGameScene();
			targetScene.Initialize(Context);
			targetScene.LoadContent();

			// Remove the current scene and add the target scene
			Context.Scenes.Remove(this);
			Context.Scenes.Add(targetScene);
		}
	}
}