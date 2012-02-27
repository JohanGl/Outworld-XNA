using System.Collections.Generic;
using Framework.Core.Contexts;
using Framework.Core.Diagnostics.Logging;
using Framework.Core.Scenes.Cameras;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Tiles;
using Game.World.Terrains.Rendering.Contexts;
using Game.World.Terrains.Visibility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.World.Terrains.Rendering
{
	/// <summary>
	/// Handles rendering of terrain areas
	/// </summary>
	public partial class TerrainRenderer
	{
		private GameContext context;
		private RenderStateContext RenderStateContext;
		private Dictionary<TileType, List<TerrainMesh>> VisibleTerrainMeshes;
		private TerrainVisibility terrainVisibility;
		private Area area;
		private TerrainMesh mesh;

		public void Initialize(TerrainVisibility terrainVisibility, GameContext context)
		{
			this.terrainVisibility = terrainVisibility;
			this.context = context;

			RenderStateContext = new RenderStateContext();
			VisibleTerrainMeshes = new Dictionary<TileType, List<TerrainMesh>>();

			Logger.RegisterLogLevelsFor<TerrainRenderer>(Logger.LogLevels.Adaptive);

			// Start the worker thread
			ThreadingContext.Start(AddAsyncLogic);
		}

		public void Clear()
		{
			VisibleTerrainMeshes.Clear();
		}

		public void Render(GameTime gameTime, CameraBase camera)
		{
			// Clear old content
			VisibleTerrainMeshes.Clear();

			// Loop through all areas to see which ones should be rendered
			for (int i = 0; i < terrainVisibility.AreaCollection.Areas.Count; i++)
			{
				area = terrainVisibility.AreaCollection.Areas[i];

				// Skip areas not visible to the camera
				if (!terrainVisibility.IsAreaVisible(area, camera))
				{
					continue;
				}

				// Loop through all meshes within the current area and add them to the visible groups
				foreach (KeyValuePair<TileType, TerrainMesh> meshPair in area.Model.Meshes)
				{
					// Initialize the current mesh-group key if needed
					if (!VisibleTerrainMeshes.ContainsKey(meshPair.Key))
					{
						VisibleTerrainMeshes.Add(meshPair.Key, new List<TerrainMesh>());
					}

					meshPair.Value.Offset.X = area.Info.Location.X * Area.Size.X;
					meshPair.Value.Offset.Y = (area.Info.Location.Y * Area.Size.Y);
					meshPair.Value.Offset.Z = area.Info.Location.Z * Area.Size.Z;

					// Add the mesh to the visible groups
					VisibleTerrainMeshes[meshPair.Key].Add(meshPair.Value);
				}
			}

			// Initiate rendering
			var device = context.Graphics.Device;
			var effect = context.Graphics.Effect as BasicEffect;
			RenderStateContext.SetRenderStates(device);

			// Loop through all visible meshes
			foreach (var meshCollection in VisibleTerrainMeshes)
			{
				// Set the current texture
				effect.Texture = context.Resources.Textures["Tile" + ((ushort)meshCollection.Key).ToString()];

				// Loop through all meshes using the current texture
				for (int i = 0; i < meshCollection.Value.Count; i++)
				{
					mesh = meshCollection.Value[i];

					// Skip empty meshes
					if (mesh.VertexCount == 0 || mesh.VertexBuffer == null)
					{
						continue;
					}

					// Set the vertex and index data to be rendered
					device.SetVertexBuffer(mesh.VertexBuffer);
					device.Indices = mesh.IndexBuffer;

					// Translate the mesh
					effect.World = Matrix.CreateTranslation(mesh.Offset);

					// Loop through all effect passes
					for (int j = 0; j < context.Graphics.Effect.CurrentTechnique.Passes.Count; j++)
					{
						// Apply the current effect pass
						context.Graphics.Effect.CurrentTechnique.Passes[j].Apply();

						// Render the mesh
						device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
													 0,
													 0,
													 mesh.VertexCount,
													 0,
													 mesh.IndexCount / 3);
						
					}
				}
			}
		}
	}
}