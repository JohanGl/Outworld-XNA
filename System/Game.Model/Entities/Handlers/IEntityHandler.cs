using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Model.Entities
{
	public interface IEntityHandler
	{
		List<IEntity> Entities { get; set; }

		void Update(GameTime gameTime);
		void Render(GameTime gameTime);

		void UpdateSingleEntity(IEntity entity, GameTime gameTime);
		void RenderSingleEntity(IEntity entity, GameTime gameTime);
	}
}