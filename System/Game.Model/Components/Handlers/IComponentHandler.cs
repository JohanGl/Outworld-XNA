using Microsoft.Xna.Framework;

namespace Game.Model.Components
{
	public interface IComponentHandler
	{
		void Add(IComponent component);
		T Get<T>() where T : class, IComponent;
		void Remove<T>() where T : class, IComponent;

		void Update(GameTime gameTime);
		void Render(GameTime gameTime);

		void UpdateSingleComponent(IComponent component, GameTime gameTime);
		void RenderSingleComponent(IComponent component, GameTime gameTime);
	}
}