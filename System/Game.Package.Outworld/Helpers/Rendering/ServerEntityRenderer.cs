using System.Collections.Generic;
using Framework.Animations;
using Framework.Core.Contexts;
using Framework.Core.Scenes.Cameras;
using Game.Network.Common;
using Microsoft.Xna.Framework;
using ServerEntity = Game.Network.Clients.ServerEntity;

namespace Outworld.Helpers.Rendering
{
	public class ServerEntityRenderer
	{
		private GameContext context;
		private Dictionary<ushort, SkinnedModel> renderedClients;

		public ServerEntityRenderer(GameContext context)
		{
			this.context = context;
			renderedClients = new Dictionary<ushort, SkinnedModel>();
		}

		public void Update(GameTime gameTime)
		{
			foreach (var client in renderedClients)
			{
				client.Value.Update(gameTime);
			}
		}

		public void Render(CameraBase camera, List<ServerEntity> serverEntities)
		{
			// Loop through all server entities to be rendered
			for (int i = 0; i < serverEntities.Count; i++)
			{
				var entity = serverEntities[i];

				// Find out what type of entity to render
				switch (entity.Type)
				{
					case EntityType.Client:
						RenderClient(camera, entity);
						break;
				}
			}
		}

		private void RenderClient(CameraBase camera, ServerEntity entity)
		{
			// Add the current client if its new
			if (!renderedClients.ContainsKey(entity.Id))
			{
				AddClient(entity);
			}

			// Change animation?
			if (entity.Animation != entity.PreviousAnimation)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Changed animation for client {0} from animation {1} to {2}: ", entity.Id, entity.PreviousAnimation, entity.Animation));
				ChangeClientAnimation(entity);
			}

			// Render the client
			renderedClients[entity.Id].Render(camera.View, camera.Projection, entity.Position + new Vector3(0, -0.725f, 0), entity.Angle.X);
		}

		private void AddClient(ServerEntity entity)
		{
			var skinnedModel = new SkinnedModel();
			skinnedModel.Initialize(context.Resources.Models["Player"]);
			skinnedModel.SetAnimationClip("Idle");

			renderedClients.Add(entity.Id, skinnedModel);
		}

		private void ChangeClientAnimation(ServerEntity entity)
		{
			if (entity.Animation >= (byte)EntityEventType.RunDirection1 && entity.Animation <= (byte)EntityEventType.RunDirection8)
			{
				renderedClients[entity.Id].SetAnimationClip("Run");
			}
			else
			{
				renderedClients[entity.Id].SetAnimationClip("Idle");
			}

			// Mark the animation as changed
			entity.PreviousAnimation = entity.Animation;
		}
	}
}