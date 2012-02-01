using System;
using System.Collections.Generic;
using System.Diagnostics;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Core.Messaging;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Game.Entities;
using Game.Network.Clients;
using Game.Network.Common;
using Game.Network.Servers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Outworld.Scenes.Debug.Network
{
	public class NetworkScene : SceneBase
	{
		private IGameServer gameServer;
		private IGameClient gameClient;
		private GameTimer sendTimer;
		private IMessageHandler messageHandler;
		private SpriteBatch spriteBatch;

		private Vector2 interpolate;
		private List<Snapshot> snapShots;
		private Texture2D baseImage;
		private Texture2D pointImage;
		private int state = 0;
		private string time;
//		1. List<pos + DateTime (class)> // högre och högre tid på olika pos
//		2. bild i ps, rita cirklar dra streck mellan dom..
//		3. bygga på med vinklar (sist, till att börja med kör vi med position)

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);
			
			snapShots = new List<Snapshot>();
			snapShots.Add(new Snapshot(TimeSpan.FromSeconds(0), new Vector2(487.0f, 585.0f)));
			snapShots.Add(new Snapshot(TimeSpan.FromSeconds(3), new Vector2(510.0f, 200.0f)));
			snapShots.Add(new Snapshot(TimeSpan.FromSeconds(7), new Vector2(775.0f, 326.0f)));
			snapShots.Add(new Snapshot(TimeSpan.FromSeconds(10), new Vector2(1142.0f, 103.0f)));

			this.spriteBatch = spriteBatch ?? new SpriteBatch(Context.Graphics.Device);

			time = "";
			interpolate = new Vector2();

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
			var content = Context.Resources.Content;
			var resources = Context.Resources;

			resources.Textures.Add("Interpolation", content.Load<Texture2D>(@"Gui\Scenes\Network\Interpolation"));
			resources.Textures.Add("Point", content.Load<Texture2D>(@"Gui\Scenes\InGame\RadarEntity"));

			baseImage = resources.Textures["Interpolation"];
			pointImage = resources.Textures["Point"];
		}

		public override void UnloadContent()
		{
			var resources = Context.Resources;

			resources.Textures.Remove("Interpolation");
			resources.Textures.Remove("Point");
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
				Random random = new Random();
				TimeSpan randomTime = TimeSpan.FromSeconds(random.NextDouble() * 10.0f);

				time = randomTime.ToString();

				GetNearestSnapshot(randomTime);
			}
		}

		private void GetNearestSnapshot(TimeSpan getNearestSnapshot)
		{
			if (snapShots[snapShots.Count-1].Time <= getNearestSnapshot)
			{
				var posA = snapShots[snapShots.Count - 2].Position;
				var posB = snapShots[snapShots.Count - 1].Position;

				interpolate = Vector2.Lerp(posA, posB, 1.0f);
			}

			for(int i = 0; i < snapShots.Count; i++)
			{
				if(getNearestSnapshot.Subtract(snapShots[i].Time).TotalSeconds < 0.0f)
				{
					var posA = snapShots[i - 1].Position;
					var posB = snapShots[i].Position;

					float smooth = (float)((getNearestSnapshot.TotalSeconds - snapShots[i - 1].Time.TotalSeconds) / (snapShots[i].Time.TotalSeconds - snapShots[i - 1].Time.TotalSeconds));

					time += " Smooth:" + smooth;

					interpolate = Vector2.Lerp(posA, posB, smooth);

					break;
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

		public override void Render(GameTime gameTime)
		{
			spriteBatch.Begin();

			Vector2 position = new Vector2(0.0f, 0.0f);
			spriteBatch.Draw(baseImage, position, Color.White);

			spriteBatch.DrawString(Context.Resources.Fonts["Global.Default"],
										"Time: " + time,
										new Vector2(3, 0),
										Color.White,
										0,
										new Vector2(0, 0),
										1,
										SpriteEffects.None,
										0);
			

			spriteBatch.Draw(pointImage, interpolate, Color.White);
			
			spriteBatch.End();
		}
	}
}