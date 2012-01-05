using System;
using Framework.Core.Contexts;
using Framework.Core.Services;
using Framework.Gui;
using Game.Network.Clients;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Outworld.Settings.Global;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class Radar : UIElement
	{
		private Texture2D RadarImage;
		private Texture2D PlayerDotImage;
		//private Texture2D EnemyDotImage;
		//private Texture2D MiscDotImage;

		private float RadarDetectionRange = 2000.0f;

		private GlobalSettings globalSettings;
		private IGameClient gameClient;

		public void Initialize(GameContext context)
		{
			globalSettings = ServiceLocator.Get<GlobalSettings>();
			gameClient = ServiceLocator.Get<IGameClient>();

			RadarImage = context.Resources.Textures["Gui.Hud.Radar"];
			PlayerDotImage = context.Resources.Textures["Gui.Hud.RadarPlayerDot"];
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Top;
			Margin = new Thickness(0, 10, 10, 0);
			Width = RadarImage.Width;
			Height = RadarImage.Height;

			guiManager.Arrange(this, availableSize);

			//positionAmount = new Vector2(Position.X + 7, Position.Y);
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(RadarImage, Position, Color.White);
			
			foreach(var client in gameClient.ServerEntities)
			{
				Vector2 diffVector = new Vector2(globalSettings.Player.Spatial.Position.X - client.Position.X, globalSettings.Player.Spatial.Position.Y - client.Position.Y);
				float distance = diffVector.LengthSquared();

				if (distance <= (RadarDetectionRange * RadarDetectionRange))
				{
					diffVector *= ((Width * 0.5f) / RadarDetectionRange);

					//diffVector = Vector2.Transform(diffVector, Matrix.CreateRotationZ(globalSettings.Player.Spatial.Angle.Z));

					diffVector += Position;

					spriteBatch.Draw(PlayerDotImage, diffVector, Color.White);
				}

				//client.Position += new Vector3(1, 1, 1);
			}

			//spriteBatch.Draw(PlayerDotImage, positionProgressBar, Color.White);
			//spriteBatch.DrawString(context.Resources.Fonts["Hud.Small"], "Weapon name", positionTitle, Color.White);
		}
	}
}