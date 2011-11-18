using System.Collections.Generic;
using Game.World.Terrains.Parts.Tiles;
using Game.World.Terrains.Rendering;
using Game.World.Terrains.Rendering.MeshPools;

namespace Game.World.Terrains.Parts.Areas
{
	public class AreaModel
	{
		public int TotalFaces;
		public int TotalVisibleFaces;
		public Dictionary<TileType, TerrainMesh> Meshes;

		public AreaModel()
		{
			Meshes = new Dictionary<TileType, TerrainMesh>();
		}

		/// <summary>
		/// Removes the mesh data in a memory safe way
		/// </summary>
		public void Clear()
		{
			foreach (var mesh in Meshes)
			{
				TerrainMeshPool.ReleaseItem(mesh.Value);
			}

			TotalFaces = 0;
			TotalVisibleFaces = 0;
			Meshes.Clear();
		}
	}
}