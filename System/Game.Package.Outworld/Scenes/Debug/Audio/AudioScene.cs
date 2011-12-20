using System;
using Framework.Audio;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Core.Scenes;
using Framework.Core.Services;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.Debug.Audio
{
	public class AudioScene : SceneBase
	{
		private IAudioHandler audioHandler;
		private GameTimer soundTimer;
		private float angle;

		public override void Initialize(GameContext context)
		{
			base.Initialize(context);

			audioHandler = ServiceLocator.Get<IAudioHandler>();
			soundTimer = new GameTimer(TimeSpan.FromMilliseconds(500));
		}

		public override void LoadContent()
		{
			var content = Context.Resources.Content;
			audioHandler.LoadSound("Sound1", @"Audio\Sounds\Gui\Notification02");
		}

		public override void UnloadContent()
		{
			audioHandler.UnloadContent();
		}

		public override void Update(GameTime gameTime)
		{
			if (soundTimer.Update(gameTime))
			{
				angle += 15f;
				float x = 50f * (float)Math.Sin(MathHelper.ToRadians((int)angle));

				audioHandler.UpdateListener(new Vector3(0, 0, 0));
				audioHandler.PlaySound3d("a", "Sound1", 1f, new Vector3(x, 0, 0));
			}

			audioHandler.Update(gameTime);
		}

		public override void Render(GameTime gameTime)
		{
		}
	}
}