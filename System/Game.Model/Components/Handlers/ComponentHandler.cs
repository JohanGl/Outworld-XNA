using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Model.Components
{
	public class ComponentHandler : IComponentHandler
	{
		private List<IComponent> components;

		public ComponentHandler()
		{
			components = new List<IComponent>();
		}

		public void Add(IComponent component)
		{
			components.Add(component);
		}

		public T Get<T>() where T : class, IComponent
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (components[i] is T)
				{
					return components[i] as T;
				}
			}

			return null;
		}

		public void Remove<T>() where T : class, IComponent
		{
			var component = Get<T>();

			if (component != null)
			{
				components.Remove(component);
			}
		}

		public void Update(GameTime gameTime)
		{
			IModelUpdateable component;

			for (int i = 0; i < components.Count; i++)
			{
				component = components[i] as IModelUpdateable;

				if (component != null)
				{
					component.Update(gameTime);
				}
			}
		}

		public void Render(GameTime gameTime)
		{
			IModelRenderable component;

			for (int i = 0; i < components.Count; i++)
			{
				component = components[i] as IModelRenderable;

				if (component != null)
				{
					component.Render(gameTime);
				}
			}
		}

		public void UpdateSingleComponent(IComponent component, GameTime gameTime)
		{
			var updateableComponent = component as IModelUpdateable;

			if (updateableComponent != null)
			{
				updateableComponent.Update(gameTime);
			}
		}

		public void RenderSingleComponent(IComponent component, GameTime gameTime)
		{
			var renderableComponent = component as IModelRenderable;

			if (renderableComponent != null)
			{
				renderableComponent.Render(gameTime);
			}
		}
	}
}