using System.Collections.Generic;
using Game.Entities.System.ComponentModel.Handlers;
using Game.Entities.System.EntityModel;

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