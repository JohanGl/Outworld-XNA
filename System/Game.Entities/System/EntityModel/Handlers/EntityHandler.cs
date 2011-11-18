using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Entities.System.EntityModel.Handlers
{
	public class EntityHandler : IEntityHandler
	{
		public List<IEntity> Entities { get; set; }

		public EntityHandler()
		{
			Entities = new List<IEntity>();
		}

		public void Update(GameTime gameTime)
		{
			IModelUpdateable entity;

			for (int i = 0; i < Entities.Count; i++)
			{
				entity = Entities[i] as IModelUpdateable;

				if (entity != null)
				{
					entity.Update(gameTime);
				}
			}
		}

		public void Render(GameTime gameTime)
		{
			IModelRenderable entity;

			for (int i = 0; i < Entities.Count; i++)
			{
				entity = Entities[i] as IModelRenderable;

				if (entity != null)
				{
					entity.Render(gameTime);
				}
			}
		}

		public void UpdateSingleEntity(IEntity entity, GameTime gameTime)
		{
			var updateableEntity = entity as IModelUpdateable;

			if (updateableEntity != null)
			{
				updateableEntity.Update(gameTime);
			}
		}

		public void RenderSingleEntity(IEntity entity, GameTime gameTime)
		{
			var renderableEntity = entity as IModelRenderable;

			if (renderableEntity != null)
			{
				renderableEntity.Render(gameTime);
			}
		}
	}
}