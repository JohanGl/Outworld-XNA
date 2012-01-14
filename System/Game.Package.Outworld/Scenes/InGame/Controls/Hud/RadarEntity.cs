using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.InGame.Controls.Hud
{
	public class RadarEntity
	{
		public int Id;
		public Vector3 Position;
		public RadarEntityColor Color;

		public enum RadarEntityColor
		{
			Yellow,
			Red
		};

		public void Initialize(int id, RadarEntityColor color, Vector3 position)
		{
			Id = id;
			Color = color;
			Position = position;
		}
	}
}