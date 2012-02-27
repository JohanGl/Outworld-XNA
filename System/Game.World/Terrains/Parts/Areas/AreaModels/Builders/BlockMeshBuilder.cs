using Framework.Core.Common;
using Game.World.Terrains.Parts.Tiles;
using Game.World.Terrains.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Game.World.Terrains.Parts.Areas.AreaModels.Builders
{
	public class BlockMeshBuilder
	{
		private Color color;
		private BoundingBox textureBox;
		private Vector2 textureScale;
		private AreaModelBuildInfo buildInfo;
		private TerrainMeshBuildInfo[] meshLookup;
		private TerrainMeshBuildInfo meshInfo;
		private int index;
		private float textureOffsetY;
		private float x2, y2, z2;

		public BlockMeshBuilder()
		{
			textureScale = new Vector2(4, 4);
			color = new Color(255, 255, 255, 255);
		}

		public void Initialize(ref AreaModelBuildInfo buildInfo, ref TerrainMeshBuildInfo[] meshLookup)
		{
			this.buildInfo = buildInfo;
			this.meshLookup = meshLookup;
		}

		public void Render(int tileType, int x, int y, int z)
		{
			// Assign the mesh data to be processed
			meshInfo = meshLookup[tileType];

			// Calculate the texture coordinates for this block based on its location within the area
			textureOffsetY = -(Tile.Size.Y / Area.Size.Y);
			textureBox.Min.X = (x / (float)Area.Size.X) * textureScale.X;
			textureBox.Min.Y = ((y / (float)Area.Size.Y) + textureOffsetY) * textureScale.Y;
			textureBox.Min.Z = (z / (float)Area.Size.Z) * textureScale.X;
			textureBox.Max.X = ((x + 1) / (float)Area.Size.X) * textureScale.X;
			textureBox.Max.Y = (((y - 1) / (float)Area.Size.Y) + textureOffsetY) * textureScale.Y;
			textureBox.Max.Z = ((z + 1) / (float)Area.Size.Z) * textureScale.X;

			y++;

			// Render all visible sides
			if (buildInfo.TileVisibleSides[(int)TileSide.Top])
			{
				AddTop(x, y, z);
			}
			
			if (buildInfo.TileVisibleSides[(int)TileSide.Bottom])
			{
				AddBottom(x, y, z);
			}
			
			if (buildInfo.TileVisibleSides[(int)TileSide.Left])
			{
				AddLeft(x, y, z);
			}
			
			if (buildInfo.TileVisibleSides[(int)TileSide.Right])
			{
				AddRight(x, y, z);
			}
			
			if (buildInfo.TileVisibleSides[(int)TileSide.Front])
			{
				AddFront(x, y, z);
			}
			
			if (buildInfo.TileVisibleSides[(int)TileSide.Back])
			{
				AddBack(x, y, z);
			}
		}

		private void AddTop(float x, float y, float z)
		{
			index = meshInfo.VertexCount;

			color.R = color.G = color.B = 255;

			x2 = x + Tile.Size.X;
			z2 = z + Tile.Size.Z;

			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z, textureBox.Min.X, textureBox.Min.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x2, y, z, textureBox.Max.X, textureBox.Min.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x2, y, z2, textureBox.Max.X, textureBox.Max.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z2, textureBox.Min.X, textureBox.Max.Z);

			meshInfo.Indices[meshInfo.IndexCount++] = index;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 2;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
		}

		private void AddBottom(float x, float y, float z)
		{
			index = meshInfo.VertexCount;

			color.R = color.G = color.B = 255;

			y -= Tile.Size.Y;

			x2 = x + Tile.Size.X;
			z2 = z + Tile.Size.Z;

			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z, textureBox.Min.X, textureBox.Min.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x2, y, z, textureBox.Max.X, textureBox.Min.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x2, y, z2, textureBox.Max.X, textureBox.Max.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z2, textureBox.Min.X, textureBox.Max.Z);

			meshInfo.Indices[meshInfo.IndexCount++] = index;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 2;
		}

		private void AddLeft(float x, float y, float z)
		{
			index = meshInfo.VertexCount;

			color.R = color.G = color.B = 185;

			y2 = y - Tile.Size.Y;
			z2 = z + Tile.Size.Z;

			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z, textureBox.Min.Y, textureBox.Min.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z2, textureBox.Min.Y, textureBox.Max.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y2, z2, textureBox.Max.Y, textureBox.Max.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y2, z, textureBox.Max.Y, textureBox.Min.Z);

			meshInfo.Indices[meshInfo.IndexCount++] = index;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 2;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
		}

		private void AddRight(float x, float y, float z)
		{
			index = meshInfo.VertexCount;

			color.R = color.G = color.B = 185;

			x += Tile.Size.X;

			y2 = y - Tile.Size.Y;
			z2 = z + Tile.Size.Z;

			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z, textureBox.Min.Y, textureBox.Min.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z2, textureBox.Min.Y, textureBox.Max.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y2, z2, textureBox.Max.Y, textureBox.Max.Z);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y2, z, textureBox.Max.Y, textureBox.Min.Z);

			meshInfo.Indices[meshInfo.IndexCount++] = index;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 2;
		}

		private void AddFront(float x, float y, float z)
		{
			index = meshInfo.VertexCount;

			color.R = color.G = color.B = 220;

			z += Tile.Size.Z;

			x2 = x + Tile.Size.X;
			y2 = y - Tile.Size.Y;

			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z, textureBox.Min.X, textureBox.Min.Y);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x2, y, z, textureBox.Max.X, textureBox.Min.Y);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x2, y2, z, textureBox.Max.X, textureBox.Max.Y);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y2, z, textureBox.Min.X, textureBox.Max.Y);

			meshInfo.Indices[meshInfo.IndexCount++] = index;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 2;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
		}

		private void AddBack(float x, float y, float z)
		{
			index = meshInfo.VertexCount;

			color.R = color.G = color.B = 220;

			x2 = x + Tile.Size.X;
			y2 = y - Tile.Size.Y;

			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y, z, textureBox.Min.X, textureBox.Min.Y);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x2, y, z, textureBox.Max.X, textureBox.Min.Y);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x2, y2, z, textureBox.Max.X, textureBox.Max.Y);
			meshInfo.Vertices[meshInfo.VertexCount++] = GetVertex(x, y2, z, textureBox.Min.X, textureBox.Max.Y);

			meshInfo.Indices[meshInfo.IndexCount++] = index;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 1;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 3;
			meshInfo.Indices[meshInfo.IndexCount++] = index + 2;
		}

		private TerrainVertex GetVertex(float x, float y, float z, float u, float v)
		{
			return new TerrainVertex()
			{
			    Position = new Vector4b((byte)x, (byte)y, (byte)z, 1),
			    Color = color,
			    TextureCoordinate = new HalfVector2(u, v)
			};
		}
	}
}