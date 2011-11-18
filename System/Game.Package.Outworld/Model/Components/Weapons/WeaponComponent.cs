using Game.Model.Components;
using Game.Model.Entities;

namespace Outworld.Model.Components.Weapons
{
	public class WeaponComponent : IComponent
	{
		public string Id { get; set; }
		public IEntity Owner { get; set; }

		public string Name { get; set; }
	
		public int Ammo { get; set; }
		public int MaximumAmmo { get; set; }

		public int AmmoInClip { get; set; }
		public int MaximumAmmoInClip { get; set; }
	}
}