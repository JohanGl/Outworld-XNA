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
		private Texture2D radarEntityImage;

		private Vector2 uiCenter;
		private Vector2 uiCenterForRadarCompass;
		private float radarDetectionRange;
		private float radarDetectionRangeSquared;
		private Vector2 radarCompassOrigin;

		public List<RadarEntity> RadarEntities;
		public Vector3 Center;
		public float Angle;

		private GameTimer fadeTimer;

		public void Initialize(GameContext context)
		{
			RadarEntities = new List<RadarEntity>();

			radarDetectionRange = 84.0f;
			radarDetectionRangeSquared = (radarDetectionRange*radarDetectionRange);

			fadeTimer = new GameTimer(TimeSpan.FromMilliseconds(50));

			radarBaseImage = context.Resources.Textures["Gui.Hud.Radar"];
			radarCompass = context.Resources.Textures["Gui.Hud.RadarCompass"];
			radarEntityImage = context.Resources.Textures["Gui.Hud.RadarEntity"];
		}

		public override void UpdateLayout(GuiManager guiManager, Rectangle availableSize)
		{
			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Top;
			Margin = new Thickness(0, 15, 15, 0);
			Width = radarBaseImage.Width;
			Height = radarBaseImage.Height;

			guiManager.Arrange(this, availableSize);

			uiCenter = new Vector2(Position.X + (Width / 2), Position.Y + (Height / 2));
			uiCenterForRadarCompass = uiCenter;

			uiCenter -= new Vector2((radarEntityImage.Width / 2.0f), (radarEntityImage.Height / 2.0f));

			radarCompassOrigin = new Vector2((radarCompass.Width / 2), (radarCompass.Height / 2));
		}

		public void Update(GameTime gameTime)
		{
			// Update all entities fade state
			if (fadeTimer.Update(gameTime))
			{
				for (int i = 0; i < RadarEntities.Count; i++)
				{
					var entity = RadarEntities[i];
					Vector2 diffVect = new Vector2(entity.Position.X - Center.X, entity.Position.Z - Center.Z);
					float distance = diffVect.LengthSquared();
					

					if (distance >= radarDetectionRangeSquared)
					{
						if(entity.LockedPosition.X == -1 && 
							entity.LockedPosition.Y == -1)
						{
							entity.LockedPosition = diffVect;
						}

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
						if (entity.LockedPosition.X != -1 &&
							entity.LockedPosition.Y != -1)
						{
							entity.LockedPosition = new Vector2(-1, -1);
						}

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

			RenderRadarEntities(device, spriteBatch, radian);

			spriteBatch.Draw(radarCompass, uiCenterForRadarCompass, null, Color.White * 1.0f, radian, radarCompassOrigin, 1, SpriteEffects.None, 0);
		}

		public void RenderRadarEntities(GraphicsDevice device, SpriteBatch spriteBatch, float radian)
		{
			for (int i = 0; i < RadarEntities.Count; i++)
			{
				var entity = RadarEntities[i];

				Vector2 diffVect = new Vector2();

				if (entity.LockedPosition.X != -1 &&
					entity.LockedPosition.Y != -1)
				{
					diffVect = entity.LockedPosition;
				}
				else
				{
					diffVect = new Vector2(entity.Position.X - Center.X, entity.Position.Z - Center.Z);
				}

				diffVect *= ((Width / 2) / radarDetectionRange);

				diffVect = Vector2.Transform(diffVect, Matrix.CreateRotationZ(radian));

				diffVect += uiCenter;

				spriteBatch.Draw(radarEntityImage, diffVect, entity.Color * entity.Opacity);
			}
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