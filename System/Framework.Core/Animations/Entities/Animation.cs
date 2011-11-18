using System;

namespace Framework.Core.Animations
{
	public class Animation
	{
		/// <summary>
		/// Gets the flag indicating whether the animation is running or not
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Gets the start value of the animation
		/// </summary>
		public float From { get; set; }

		/// <summary>
		/// Gets the end value of the animation
		/// </summary>
		public float To { get; set; }

		/// <summary>
		/// Gets the duration in milliseconds
		/// </summary>
		public int Duration { get; set; }

		/// <summary>
		/// Gets the current animation time in milliseconds
		/// </summary>
		public int CurrentMilliseconds { get; private set; }

		/// <summary>
		/// Gets the current value based on the From and To values during a running animation
		/// </summary>
		public float CurrentValue { get; private set; }

		/// <summary>
		/// Gets the current animation completion percentage
		/// </summary>
		public float Percentage { get; private set; }

		/// <summary>
		/// Gets the current animation completion percentage in a 0-1 interval
		/// </summary>
		public float PercentageNormalized { get; private set; }

		/// <summary>
		/// Keeps track of when the animation was last started
		/// </summary>
		private DateTime started;

		public Animation(float from, float to, TimeSpan duration)
		{
			From = from;
			To = to;
			Duration = (int)duration.TotalMilliseconds;
		}

		public Animation(float from, float to, int milliseconds)
		{
			From = from;
			To = to;
			Duration = milliseconds;
		}

		/// <summary>
		/// Starts the animation
		/// </summary>
		public void Start()
		{
			Clear();
			started = DateTime.Now;
			IsRunning = true;
			Update();
		}

		/// <summary>
		/// Updates a running animation
		/// </summary>
		public void Update()
		{
			if (IsRunning)
			{
				if (CurrentMilliseconds < Duration)
				{
					// Update the current animation time
					CurrentMilliseconds = (int)(DateTime.Now - started).TotalMilliseconds;

					// Check if this is the last pass of the running animation before it should stop
					if (CurrentMilliseconds > Duration)
					{
						CurrentMilliseconds = Duration;
					}

					PercentageNormalized = (float)CurrentMilliseconds / Duration;
					Percentage = PercentageNormalized * 100f;

					// Update the current value
					CurrentValue = From - ((From - To) * PercentageNormalized);
				}
				else
				{
					Stop();
				}
			}
		}

		/// <summary>
		/// Stops the animation
		/// </summary>
		public void Stop()
		{
			IsRunning = false;
			Clear();
		}

		/// <summary>
		/// Resets the animation state
		/// </summary>
		private void Clear()
		{
			Percentage = 0;
			PercentageNormalized = 0;
			CurrentMilliseconds = 0;
			CurrentValue = From;
			started = DateTime.MinValue;
		}
	}
}