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
		private Vector2 radarEntityOffset;

		public List<RadarEntity> RadarEntities;
		public Vector3 Center;
		public float Angle;
		private float radarDetectionRange = 50.0f;

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
			radarEntityOffset = new Vector2((-radarPlayerTypeEntityImage.Width/2.0f), (-radarPlayerTypeEntityImage.Height/2.0f));
		}

		public override void Render(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(radarBaseImage, Position, Color.White);

			var radian = MathHelper.ToRadians(Angle);

			for (int i = 0; i < RadarEntities.Count; i++)
			{
				//var entity = RadarEntities[i];

				//var result = RotateAroundPoint(entity.Position, Center, new Vector3(1, 0, 0), radian);
				//result = RotateAroundPoint(result, Center, new Vector3(0, 1, 0), radian);

				//if (entity.Color == RadarEntity.RadarEntityColor.Yellow)
				//{
				//    Vector2 position = new Vector2(uiCenter.X + result.X, uiCenter.Y + result.Z);
				//    spriteBatch.Draw(radarPlayerTypeEntityImage, position, Color.White);
				//}






				//var entity = RadarEntities[i];
				//Vector2 diffVect = new Vector2(entity.Position.X - Center.X, entity.Position.Z - Center.Z);
				//float distance = diffVect.LengthSquared();

				//// Check if enemy is within RadarRange
				//if (distance < (radarDetectionRange*radarDetectionRange))
				//{
				//    // Scale the distance from world coords to radar coords
				//    diffVect *= ((Width * 0.5f) / radarDetectionRange);

				//    // We rotate each point on the radar so that the player is always facing UP on the radar
				//    diffVect = Vector2.Transform(diffVect, Matrix.CreateRotationZ(radian));

				//    // Offset coords from radar's center
				//    diffVect += new Vector2(Center.X + (Width * 0.5f), Center.Z + (Height * 0.5f));

				//    //// We scale each dot so that enemies that are at higher elevations have bigger dots, and enemies
				//    //// at lower elevations have smaller dots.
				//    //float scaleHeight = 1.0f + ((entity.Position.Y - Center.Y) / 200.0f);

				//    //// Draw enemy dot on radar
				//    //spriteBatch.Draw(radarPlayerTypeEntityImage, diffVect + Position, null, Color.White, 0.0f, new Vector2(0.0f, 0.0f), scaleHeight, SpriteEffects.None, 0.0f);

				//    spriteBatch.Draw(radarPlayerTypeEntityImage, diffVect + Position, Color.White);
				//}
				

				var entity = RadarEntities[i];
				Vector2 diffVect = new Vector2(entity.Position.X - Center.X, entity.Position.Z - Center.Z);
				float distance = diffVect.LengthSquared();

				// Check if enemy is within RadarRange
				if (distance < (radarDetectionRange * radarDetectionRange))
				{
					// Scale the distance from world coords to radar coords
					diffVect *= ((Width / 2) / radarDetectionRange);														//			RadarScreenRadius / RadarRange;

					// We rotate each point on the radar so that the player is always facing UP on the radar
					diffVect = Vector2.Transform(diffVect, Matrix.CreateRotationZ(radian));

					// Offset coords from radar's center
					diffVect += uiCenter;

					spriteBatch.Draw(radarPlayerTypeEntityImage, diffVect + radarEntityOffset, Color.White);
				}
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
		public Vector3 RotateAroundPoint(Vector3 point, Vector3 originPoint, Vector3 rotationAxis, float radiansToRotate)
		{
			Vector3 diffVect = point - originPoint;

			Vector3 rotatedVect = Vector3.Transform(diffVect, Matrix.CreateFromAxisAngle(rotationAxis, radiansToRotate));

			rotatedVect += originPoint;

			return rotatedVect;
		}
	}
}