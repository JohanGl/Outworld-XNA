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
		private float radarOtherDetectionRange;
		private Vector2 radarCompassOrigin;

		public List<RadarEntity> RadarEntities;
		public Vector3 Center;
		public float Angle;

		public Radar(float detectionRange)
		{
			radarDetectionRange = detectionRange;
		}

		public void Initialize(GameContext context)
		{
			RadarEntities = new List<RadarEntity>();

			radarOtherDetectionRange = radarDetectionRange + 10.0f;

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
			for (int i = 0; i < RadarEntities.Count; i++)
			{
				var entity = RadarEntities[i];
				Vector2 diffVect = new Vector2(entity.Position.X - Center.X, entity.Position.Z - Center.Z);
				float distance = diffVect.Length();

				if (distance >= radarDetectionRange)
				{
					entity.Position2D = Vector2.Normalize(diffVect);
					entity.Position2D *= radarDetectionRange;

//					entity.Opacity = 0.5f;
//					Console.WriteLine(distance.ToString() + " <= " radarOtherDetectionRange);
					if (distance <= radarOtherDetectionRange)
					{
						entity.Opacity = radarOtherDetectionRange - distance;
					}
					else
					{
						entity.Opacity = 0.0f;
					}
				}
				else
				{
					entity.Position2D = diffVect;
					entity.Opacity = 1.0f;
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

				entity.Position2D *= ((Width / 2) / radarDetectionRange);

				entity.Position2D = Vector2.Transform(entity.Position2D, Matrix.CreateRotationZ(radian));

				entity.Position2D += uiCenter;

				spriteBatch.Draw(radarEntityImage, entity.Position2D, entity.Color * entity.Opacity);
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