using Game.Model.Entities;

namespace Game.Model.Components
{
	public interface IComponent
	{
		string Id { get; set; }
		IEntity Owner { get; set; }
	}
}