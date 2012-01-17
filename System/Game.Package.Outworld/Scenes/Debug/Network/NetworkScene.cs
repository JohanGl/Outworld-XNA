using System.Diagnostics;
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

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			gameServer = ServiceLocator.Get<IGameServer>();
			gameClient = ServiceLocator.Get<IGameClient>();

			// Create the connection between the client and server
			gameServer.Start();
			gameClient.Connect();
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
		}

		public override void Render(GameTime gameTime)
		{
		}
	}
}