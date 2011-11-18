using System.Collections.Generic;
using Game.Model.Components;

namespace Game.Model.Entities
{
	public interface IEntity
	{
		string Id { get; set; }
		IEntity Parent { get; set; }
		IEnumerable<IEntity> Children { get; set; }
		IComponentHandler Components { get; set; }
	}
}