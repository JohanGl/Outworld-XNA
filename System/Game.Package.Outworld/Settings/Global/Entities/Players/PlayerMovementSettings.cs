using Microsoft.Xna.Framework;

namespace Outworld.Settings.Global
{
	public class PlayerMovementSettings
	{
		public Vector2 LookAroundAmplifier { get; set; }
		public Vector2 MovementAmplifier { get; set; }
		public float CrouchingMovementReduction { get; set; }
	}
}