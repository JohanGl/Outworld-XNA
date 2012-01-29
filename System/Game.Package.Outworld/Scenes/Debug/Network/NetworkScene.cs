using System;
using System.Collections.Generic;
using System.Diagnostics;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Core.Messaging;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Game.Network.Clients;
using Game.Network.Common;
using Game.Network.Servers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Outworld.Scenes.Debug.Network
{
	public class NetworkScene : SceneBase
	{
		private IGameServer gameServer;
		private IGameClient gameClient;
		private GameTimer sendTimer;
		private IMessageHandler messageHandler;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			gameServer = ServiceLocator.Get<IGameServer>();
			gameClient = ServiceLocator.Get<IGameClient>();
			messageHandler = ServiceLocator.Get<IMessageHandler>();

			// Create the connection between the client and server
			gameServer.Start();
			gameClient.Connect();

			//sendTimer = new GameTimer(TimeSpan.FromMilliseconds(1000 / 20), SendClientSpatial);

			Context.Input.Keyboard.ClearMappings();
			Context.Input.Keyboard.AddMapping(Keys.F1);
			Context.Input.Keyboard.AddMapping(Keys.F2);
			Context.Input.Keyboard.AddMapping(Keys.F3);
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

			if (Context.Input.Keyboard.KeyboardState[Keys.F1].WasJustPressed)
			{
			}

			sendTimer.Update(gameTime);
		}

		private PlayerMessage GetPlayerMessage(ClientActionType type)
		{
			var message = new PlayerMessage();
			message.ClientAction = new ClientAction();
			message.ClientAction.TimeStamp = gameClient.TimeStamp;
			message.ClientAction.Type = type;

			return message;
		}

		public override void Render(GameTime gameTime)
		{
		}
	}
}