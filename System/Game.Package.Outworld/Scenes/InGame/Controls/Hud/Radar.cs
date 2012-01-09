using System;
using System.Collections.Generic;
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
		private Texture2D radarBaseImage;
		private Texture2D radarPlayerTypeEntetyImage;

		public List<RadarEntity> RadarEnteties;
		public Vector3 Center;

		public void Initialize(GameContext context)
		{
			RadarEnteties = new List<RadarEntity>();

			radarBaseImage = context.Resources.Textures["Gui.Hud.Radar"];
			radarPlayerTypeEntetyImage = context.Resources.Textures["Gui.Hud.RadarPlayerDot"];
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Top;
			Margin = new Thickness(0, 10, 10, 0);
			Width = radarBaseImage.Width;
			Height = radarBaseImage.Height;

			guiManager.Arrange(this, availableSize);

			Center = new Vector3(Position.X + (Width / 2), Position.Y + (Height / 2), 0);
		}


		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(radarBaseImage, Position, Color.White);

			for(int i = 0; i < RadarEnteties.Count; i++)
			{
				if (RadarEnteties[i].Color == RadarEntity.RadarEntityColor.Yellow)
				{
					Vector2 position = new Vector2(RadarEnteties[i].Position.X, RadarEnteties[i].Position.Y);
					spriteBatch.Draw(radarPlayerTypeEntetyImage, position, Color.White);
				}
			}
		}
	}
}