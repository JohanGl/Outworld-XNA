using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class RadarEntity
	{
		public int Id;
		public Vector3 Position;
		public Color Color;
		public float Opacity;

		public Vector2 LockedPosition;

		public void Initialize(int id, Color color, Vector3 position)
		{
			Id = id;
			Color = color;
			Position = position;

			LockedPosition = new Vector2(-1, -1);
		}
	}
}