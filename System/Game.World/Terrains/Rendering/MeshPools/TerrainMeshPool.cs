using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Game.World.Terrains.Rendering.MeshPools
{
	public static class TerrainMeshPool
	{
		public static string Statistics
		{
			get
			{
				return "TerrainMeshPool Total: " + pool.Count;
			}
		}

		private static GraphicsDevice device;
		private static List<TerrainMesh> pool;

		public static void Initialize(GraphicsDevice device)
		{
			TerrainMeshPool.device = device;
			pool = new List<TerrainMesh>();
		}

		public static TerrainMesh GetFreeItem(int vertices)
		{
			int currentMinimum = int.MaxValue;
			int? currentMinimumIndex = null;

			// Try to locate a free mesh as close to the desired vertex-size as possible
			for (int i = 0; i < pool.Count; i++)
			{
				if (pool[i].State == PoolItemState.Free &&
					pool[i].VertexBuffer.VertexCount >= vertices &&
					pool[i].VertexBuffer.VertexCount < currentMinimum)
				{
					currentMinimum = pool[i].VertexBuffer.VertexCount;
					currentMinimumIndex = i;
				}
			}

			// Found a suitable mesh
			if (currentMinimumIndex.HasValue)
			{
				// Return it
				pool[currentMinimumIndex.Value].State = PoolItemState.Active;
				return pool[currentMinimumIndex.Value];
			}
			// No suitable mesh found
			else
			{
				var mesh = CreateMesh(vertices);
				mesh.State = PoolItemState.Active;
				pool.Add(mesh);

				return mesh;
			}
		}

		private static TerrainMesh CreateMesh(int vertices)
		{
			// Create a new mesh and return it
			int maxIndices = (int)(vertices * 1.5d);

			var mesh = new TerrainMesh();
			mesh.State = PoolItemState.Free;
			mesh.VertexBuffer = new VertexBuffer(device, typeof(TerrainVertex), vertices, BufferUsage.WriteOnly);
			mesh.IndexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, maxIndices, BufferUsage.WriteOnly);

			return mesh;
		}

		public static void ReleaseItem(TerrainMesh mesh)
		{
			mesh.State = PoolItemState.Free;
			mesh.VertexCount = 0;
			mesh.IndexCount = 0;
		}

		public static void Clear()
		{
			foreach (var mesh in pool)
			{
				if (mesh.VertexBuffer != null)
				{
					mesh.VertexBuffer.Dispose();
					mesh.VertexBuffer = null;
				}

				if (mesh.IndexBuffer != null)
				{
					mesh.IndexBuffer.Dispose();
					mesh.IndexBuffer = null;
				}
			}

			pool.Clear();
		}

		//public static void Expand()
		//{
		//    if (pool.Count == 0)
		//    {
		//        return;
		//    }

		//    int averageVertexCount = 0;
		//    int min = int.MaxValue;
		//    int max = int.MinValue;

		//    for (int i = 0; i < pool.Count; i++)
		//    {
		//        var mesh = pool[i];
		//        averageVertexCount += mesh.VertexCount;

		//        if (mesh.VertexCount < min)
		//        {
		//            min = mesh.VertexCount;
		//        }

		//        if (mesh.VertexCount > max)
		//        {
		//            max = mesh.VertexCount;
		//        }
		//    }

		//    averageVertexCount /= pool.Count;

		//    for (int i = 0; i < 20; i++)
		//    {
		//        for (float j = 0.2f; j <= 1f; j += 0.1f)
		//        {
		//            pool.Add(CreateMesh((int)(max * j)));
		//        }
		//    }
		//}
	}
}