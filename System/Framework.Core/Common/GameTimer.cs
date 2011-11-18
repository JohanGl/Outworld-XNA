using System;
using Microsoft.Xna.Framework;

namespace Framework.Core.Common
{
	/// <summary>
	/// Provides timer functionality based on the GameTime class to check if a specified interval has been reached or not
	/// </summary>
	public class GameTimer
	{
		private double interval;
		private double currentTime;
		private Action intervalAction;

		public GameTimer()
		{
		}

		public GameTimer(TimeSpan interval, Action intervalAction = null)
		{
			Initialize(interval, intervalAction);
		}

		public void Initialize(TimeSpan interval, Action intervalAction = null)
		{
			this.interval = interval.TotalMilliseconds;
			this.intervalAction = intervalAction;
		}

		public bool Update(GameTime gameTime)
		{
			currentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

			// Check if the interval has been reached
			if (currentTime >= interval)
			{
				// If we have an action defined, then trigger it
				if (intervalAction != null)
				{
					intervalAction.Invoke();
				}

				currentTime = 0;
				return true;
			}

			return false;
		}
	}
}