using System.Collections.Generic;
using Game.Model.Components;
using Game.Model.Entities;

namespace Outworld.Model.Entities
{
	public class Entity : IEntity
	{
		public string Id { get; set; }
		public IEntity Parent { get; set; }
		public IEnumerable<IEntity> Children { get; set; }
		public IComponentHandler Components { get; set; }

		public Entity()
		{
			Components = new ComponentHandler();
		}
	}
}