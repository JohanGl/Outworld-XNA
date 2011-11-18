using System.Collections.Generic;
using Game.World.Terrains.Rendering.MeshPools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.World.Terrains.Rendering
{
	/// <summary>
	/// Defines a terrain mesh which can be rendered by the graphics device
	/// </summary>
	public class TerrainMesh
	{
		public PoolItemState State;
		public Vector3 Offset;

		public int VertexCount;
		public int IndexCount;

		public VertexBuffer VertexBuffer;
		public IndexBuffer IndexBuffer;

		public TerrainMesh()
		{
			State = PoolItemState.Free;
		}
	}
}