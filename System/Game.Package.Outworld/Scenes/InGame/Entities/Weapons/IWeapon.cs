namespace Outworld.Scenes.InGame.Entities.Weapons
{
	public abstract class Weapon
	{
		public int Ammo { get; private set; }
		public int AmmoCapacity { get; private set; }
		public int AmmoClipSize { get; private set; }
		public int AmmoInClip { get; private set; }

		public double InitialFireDelay { get; private set; }
		public double FireDelay { get; private set; }

		public abstract void Fire();
		public abstract void Reload();
	}

	public abstract class MeleeWeapon : Weapon
	{
	}

	public abstract class ProjectileWeapon : Weapon
	{
	}
}