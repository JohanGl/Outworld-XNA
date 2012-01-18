using System;
using System.Diagnostics;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Game.Network.Clients;
using Game.Network.Servers;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.Debug.Network
{
	public class NetworkScene : SceneBase
	{
		private IGameServer gameServer;
		private IGameClient gameClient;
		private GameTimer sendTimer;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			gameServer = ServiceLocator.Get<IGameServer>();
			gameClient = ServiceLocator.Get<IGameClient>();

			// Create the connection between the client and server
			gameServer.Start();
			gameClient.Connect();

			sendTimer = new GameTimer(TimeSpan.FromMilliseconds(1000 / 20), SendClientSpatial);
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

			if (!gameClient.IsConnected)
			{
				return;
			}

			sendTimer.Update(gameTime);
		}

		public override void Render(GameTime gameTime)
		{
		}

		private void SendClientSpatial()
		{
			gameClient.SendClientSpatial(new Vector3(0), new Vector3(1), new Vector3(2));
		}
	}
}