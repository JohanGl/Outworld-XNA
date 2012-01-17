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

		public RadarEntity()
		{
			LockedPosition = new Vector2(-1, -1);
		}
	}
}