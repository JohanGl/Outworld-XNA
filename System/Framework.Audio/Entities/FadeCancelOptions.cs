namespace Framework.Audio
{
	/// <summary>
	/// Options for AudioManager.CancelFade
	/// </summary>
	public enum FadeCancelOptions
	{
		/// <summary>
		/// Return to pre-fade volume
		/// </summary>
		Source,
		/// <summary>
		/// Snap to fade target volume
		/// </summary>
		Target,
		/// <summary>
		/// Keep current volume
		/// </summary>
		Current
	}
}