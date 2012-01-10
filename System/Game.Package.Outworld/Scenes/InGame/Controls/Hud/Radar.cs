using System.Collections.Generic;
using Framework.Core.Contexts;
using Framework.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class Radar : UIElement
	{
		private Texture2D radarBaseImage;
		private Texture2D radarPlayerTypeEntityImage;
		private Vector2 uiCenter;

		public List<RadarEntity> RadarEntities;
		public Vector3 Center;
		public float Angle;

		public void Initialize(GameContext context)
		{
			RadarEntities = new List<RadarEntity>();

			radarBaseImage = context.Resources.Textures["Gui.Hud.Radar"];
			radarPlayerTypeEntityImage = context.Resources.Textures["Gui.Hud.RadarPlayerDot"];
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Top;
			Margin = new Thickness(0, 10, 10, 0);
			Width = radarBaseImage.Width;
			Height = radarBaseImage.Height;

			guiManager.Arrange(this, availableSize);

			uiCenter = new Vector2(Position.X + (Width / 2), Position.Y + (Height / 2));
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(radarBaseImage, Position, Color.White);

			var radian = MathHelper.ToRadians(Angle);
			var radarRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(radian));

			for (int i = 0; i < RadarEntities.Count; i++)
			{
				var entity = RadarEntities[i];

				var result = entity.Position - Center;

				//result = Vector3.Transform(result, radarRotation);
				//var result = RotateAroundPoint(test, Center, new Vector3(1, 1, 0), MathHelper.ToRadians(Angle));

				if (entity.Color == RadarEntity.RadarEntityColor.Yellow)
				{
					Vector2 position = new Vector2(uiCenter.X + result.X, uiCenter.Y + result.Z);
					spriteBatch.Draw(radarPlayerTypeEntityImage, position, Color.White);
				}
			}
		}
	}
}