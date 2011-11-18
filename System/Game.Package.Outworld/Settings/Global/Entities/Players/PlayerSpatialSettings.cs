using Framework.Core.Common;
using Microsoft.Xna.Framework;

namespace Outworld.Settings.Global
{
	public class PlayerSpatialSettings
	{
		public Vector3 Position { get; set; }
		public Vector3 Angle { get; set; }
		public Vector3 Velocity { get; set; }
		public Vector3i Area { get; set; }
		public Vector3 Size { get; set; }
		public Vector3 CollisionDetectionBounds { get; set; }
	}
}