using System.Collections.Generic;
using Framework.Audio;
using Framework.Core.Contexts;
using Framework.Core.Messaging;
using Framework.Core.Packages;
using Framework.Core.Services;
using Game.Network.Clients;
using Game.Network.Clients.Settings;
using Game.Network.Servers;
using Game.Network.Servers.Settings;
using Microsoft.Xna.Framework.Graphics;
using Outworld.Scenes.Debug.Audio;
using Outworld.Scenes.Debug.Models;
using Outworld.Scenes.Debug.Terrain;
using Outworld.Scenes.InGame;
using Outworld.Scenes.MainMenu;
using Outworld.Settings;
using Outworld.Settings.Global;

namespace Outworld
{
	/// <summary>
	/// The main entry point for this assembly is this class since it inherits
	/// from IGamePackage which will be located by Game.Core at startup.
	/// </summary>
	public class GamePackage : IGamePackage
	{
		public GamePackageInfo Info { get; private set; }

		public GamePackage()
		{
			// Initialize the info of this game package
			Info = new GamePackageInfo()
			{
				Name = "Outworld",
				Description = "Description about the game...",
				Assembly = "Game.Package.Outworld.dll",
				Version = "0.1a"
			};
		}

		public void Initialize(GameContext gameContext)
		{
			// Register all globally accessible objects
			ServiceLocator.Register<GlobalSettings>(new SettingsHandler().GetGlobalSettings());
			ServiceLocator.Register<IMessageHandler>(CreateMessageHandler());
			ServiceLocator.Register<GameContext>(gameContext);
			ServiceLocator.Register<IGameServer>(CreateGameServer());
			ServiceLocator.Register<IGameClient>(CreateGameClient());
			ServiceLocator.Register<IAudioHandler>(CreateAudioHandler());

			// Initialize the default content settings
			var content = gameContext.Resources.Content;
			content.RootDirectory = "Content";
			gameContext.Resources.Fonts.Add("Global.Default", content.Load<SpriteFont>(@"Fonts\Default"));
			gameContext.Resources.Fonts.Add("Global.Normal", content.Load<SpriteFont>(@"Fonts\Moire_Bold_20"));
			gameContext.Resources.Textures.Add("Global.TerrainMergeMask", content.Load<Texture2D>(@"Terrain\TerrainMergeMask"));

			// Initialize the root scene of this game package
			//gameContext.Scenes.Add(new MainMenuScene());
			gameContext.Scenes.Add(new NewGameScene());
			//gameContext.Scenes.Add(new TerrainDebugScene());
			//gameContext.Scenes.Add(new ModelScene());
			//gameContext.Scenes.Add(new AudioScene());
		}

		public void Shutdown()
		{
			// Clean up
			ServiceLocator.Get<IGameClient>().Disconnect();
			ServiceLocator.Get<IGameServer>().Stop("Server shutdown");
			ServiceLocator.Clear();
		}

		private IGameServer CreateGameServer()
		{
			var globalSettings = ServiceLocator.Get<GlobalSettings>();

			// Initialize the settings for the server
			var serverSettings = new GameServerSettings
			{
				Port = globalSettings.Network.ServerPort,
				MaximumConnections = globalSettings.Network.MaximumConnections
			};

			serverSettings.World.Gravity = globalSettings.World.Gravity;
			serverSettings.World.Seed = globalSettings.World.Seed;

			// Create, initialize and return the server
			var server = new GameServer();
			server.Initialize(serverSettings);

			return server;
		}

		private IGameClient CreateGameClient()
		{
			var globalSettings = ServiceLocator.Get<GlobalSettings>();

			// Initialize the settings for the client
			var clientSettings = new GameClientSettings
			{
				ServerAddress = globalSettings.Network.ServerAddress,
				ServerPort = globalSettings.Network.ServerPort
			};

			// Create, initialize and return the client
			var client = new GameClient();
			client.Initialize(clientSettings);

			return client;
		}

		private IAudioHandler CreateAudioHandler()
		{
			var gameContext = ServiceLocator.Get<GameContext>();
			var globalSettings = ServiceLocator.Get<GlobalSettings>();

			var audioHandler = new DefaultAudioHandler(gameContext.Resources.Content)
			{
				MusicVolume = globalSettings.Audio.MusicVolume,
				SoundVolume = globalSettings.Audio.SoundVolume
			};

			return audioHandler;
		}

		private IMessageHandler CreateMessageHandler()
		{
			return new DefaultMessageHandler();
		}
	}
}