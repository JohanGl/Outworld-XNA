using Game.Entities.System.EntityModel;

namespace Game.Entities.System.ComponentModel
{
	public interface IComponent
	{
		string Id { get; set; }
		IEntity Owner { get; set; }
	}
}