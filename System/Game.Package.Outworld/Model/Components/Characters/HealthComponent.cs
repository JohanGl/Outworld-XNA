using Game.Model.Components;
using Game.Model.Entities;

namespace Outworld.Model.Components.Characters
{
	public class HealthComponent : IComponent
	{
		public string Id { get; set; }
		public IEntity Owner { get; set; }

		public float Health { get; private set; }
		public float MaximumHealth { get; private set; }
		public float Percentage { get; private set; }

		public void Initialize(float healthAndMaximumHealth)
		{
			SetMaximumHealth(healthAndMaximumHealth);
			SetHealth(healthAndMaximumHealth);
		}

		public void Initialize(float health, float maximumHealth)
		{
			SetMaximumHealth(maximumHealth);
			SetHealth(health);
		}

		public void SetHealth(float value)
		{
			if (value > MaximumHealth)
			{
				value = MaximumHealth;
			}
			else if (value < 0)
			{
				value = 0;
			}

			Health = value;

			UpdatePercentage();
		}

		public void SetMaximumHealth(float value)
		{
			if (value < 0)
			{
				value = 0;
			}
			
			if (Health > value)
			{
				Health = value;
			}

			MaximumHealth = value;

			UpdatePercentage();
		}

		public void Subtract(float value)
		{
			if (value > 0)
			{
				SetHealth(Health - value);
			}
		}

		private void UpdatePercentage()
		{
			if (Health == 0 || MaximumHealth == 0)
			{
				Percentage = 0;
			}

			Percentage = (Health / MaximumHealth) * 100f;
		}
	}
}