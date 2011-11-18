using Framework.Core.Common;
using Microsoft.Xna.Framework;

namespace Outworld.Settings.Global
{
	public class WorldSettings
	{
		public int Seed { get; set; }
		public bool Fog { get; set; }
		public Vector3 Gravity { get; set; }
		public Vector2i ViewDistance { get; set; }
	}
}