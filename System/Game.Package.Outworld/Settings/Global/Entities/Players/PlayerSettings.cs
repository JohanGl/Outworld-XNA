namespace Outworld.Settings.Global
{
	public class PlayerSettings
	{
		public float CameraOffsetY { get; set; }
		public float CameraCrouchingOffsetY { get; set; }
		public float ImpactDamageAmplifier { get; set; }
		public int Health { get; set; }
		public int MaxHealth { get; set; }
		public PlayerSpatialSettings Spatial { get; set; }
		public PlayerMovementSettings Movement { get; set; }
		public PlayerAnimationDurationSettings AnimationDuration { get; set; }

		public PlayerSettings()
		{
			Spatial = new PlayerSpatialSettings();
			Movement = new PlayerMovementSettings();
			AnimationDuration = new PlayerAnimationDurationSettings();
		}
	}
}