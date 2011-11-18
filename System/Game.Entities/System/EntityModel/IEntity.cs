using System.Collections.Generic;
using Game.Entities.System.ComponentModel.Handlers;

namespace Game.Entities.System.EntityModel
{
	public interface IEntity
	{
		string Id { get; set; }
		IEntity Parent { get; set; }
		IEnumerable<IEntity> Children { get; set; }
		IComponentHandler Components { get; set; }
	}
}