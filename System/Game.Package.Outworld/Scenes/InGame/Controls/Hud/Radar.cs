using System;
using System.Collections.Generic;
using Framework.Core.Common;
using Framework.Core.Contexts;
using Framework.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class Radar : UIElement
	{
		private Texture2D radarBaseImage;
		private Texture2D radarCompass;
		private Texture2D yellowRadarEntityImage;
		private Texture2D redRadarEntityImage;
		private Texture2D greenRadarEntityImage;

		private Vector2 uiCenter;
		private float radarDetectionRange;
		private float radarDetectionRangeSquared;
		private Vector2 radarOrigin;

		public List<RadarEntity> RadarEntities;
		public Vector3 Center;
		public float Angle;

		public GameTimer fadeTimer;

		public void Initialize(GameContext context)
		{
			RadarEntities = new List<RadarEntity>();

			radarDetectionRange = 84.0f;
			radarDetectionRangeSquared = (radarDetectionRange*radarDetectionRange);

			fadeTimer = new GameTimer(TimeSpan.FromMilliseconds(50));

			radarBaseImage = context.Resources.Textures["Gui.Hud.Radar"];
			radarCompass = context.Resources.Textures["Gui.Hud.RadarCompass"];
			yellowRadarEntityImage = context.Resources.Textures["Gui.Hud.YellowRadarEntity"];
			redRadarEntityImage = context.Resources.Textures["Gui.Hud.RedRadarEntity"];
			greenRadarEntityImage = context.Resources.Textures["Gui.Hud.GreenRadarEntity"];
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
			uiCenter -= new Vector2((yellowRadarEntityImage.Width / 2.0f), (yellowRadarEntityImage.Height / 2.0f));

			radarOrigin = new Vector2(Width / 2, Height / 2);
		}

		public void Update(GameTime gameTime)
		{
			// Update all items fade state
			if (fadeTimer.Update(gameTime))
			{
				for (int i = 0; i < RadarEntities.Count; i++)
				{
					var entity = RadarEntities[i];
					Vector2 diffVect = new Vector2(entity.Position.X - Center.X, entity.Position.Z - Center.Z);
					float distance = diffVect.LengthSquared();
					
					if (distance >= radarDetectionRangeSquared)
					{
						if (entity.Opacity > 0.0f)
						{
							entity.Opacity -= 0.05f;
						}
						else
						{
							entity.Opacity = 0.0f;
						}
					}
					else
					{
						if (entity.Opacity <= 1.0f)
						{
							entity.Opacity += 0.05f;
						}
						else
						{
							entity.Opacity = 1.0f;
						}
					}
				}
			}
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(radarBaseImage, Position, Color.White);

			var radian = MathHelper.ToRadians(Angle);

			for (int i = 0; i < RadarEntities.Count; i++)
			{
				var entity = RadarEntities[i];

				Vector2 diffVect = new Vector2(entity.Position.X - Center.X, entity.Position.Z - Center.Z);
				
				float distance = diffVect.LengthSquared();

//				if (distance < radarDetectionRangeSquared)
				{
					diffVect *= ((Width / 2) / radarDetectionRange);

					diffVect = Vector2.Transform(diffVect, Matrix.CreateRotationZ(radian));

					diffVect += uiCenter;

					if(entity.Color == RadarEntity.RadarEntityColor.Yellow)
					{
						spriteBatch.Draw(yellowRadarEntityImage, diffVect, Color.White * entity.Opacity);
					}
					else if(entity.Color == RadarEntity.RadarEntityColor.Red)
					{
						spriteBatch.Draw(redRadarEntityImage, diffVect, Color.White * entity.Opacity);
					}
					else
					{
						spriteBatch.Draw(greenRadarEntityImage, diffVect, Color.White * entity.Opacity);
					}
				}
			}

			spriteBatch.Draw(radarCompass, new Vector2(Position.X + (Width / 2), Position.Y + (Height / 2)), null, Color.White * 0.2f, radian, radarOrigin, 1, SpriteEffects.None, 0);
		}

		
		/// <summary>
		/// Translates a point around an origin
		/// </summary>
		/// <param name="point">Point that is going to be translated</param>
		/// <param name="originPoint">Origin of rotation</param>
		/// <param name="rotationAxis">Axis to rotate around, this Vector should be a unit vector (normalized)</param>
		/// <param name="radiansToRotate">Radians to rotate</param>
		/// <returns>Translated point</returns>
		public Vector3 RotateAroundPoint(Vector3 point, Vector3 originPoint, Vector3 rotationAxis, float radiansToRotate)				// TODO Move to framework.core
		{
			Vector3 diffVect = point - originPoint;

			Vector3 rotatedVect = Vector3.Transform(diffVect, Matrix.CreateFromAxisAngle(rotationAxis, radiansToRotate));

			rotatedVect += originPoint;

			return rotatedVect;
		}
	}
}