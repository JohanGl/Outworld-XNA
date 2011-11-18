using System;
using Framework.Core.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Game.World.Terrains.Rendering
{
	/// <summary>
	/// Contains a vertex format which is heavily optimized and adapted for the terrain mesh
	/// </summary>
	public struct TerrainVertex : IVertexType
	{
		public static readonly VertexDeclaration VertexDeclaration;
		public Vector4b Position;
		public Color Color;
		public HalfVector2 TextureCoordinate;

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get
			{
				return VertexDeclaration;
			}
		}

		static TerrainVertex()
		{
			VertexElement[] elements = new VertexElement[]
			{
				new VertexElement(0, VertexElementFormat.Byte4, VertexElementUsage.Position, 0),
				new VertexElement(4, VertexElementFormat.Color, VertexElementUsage.Color, 0),
				new VertexElement(8, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 0)
			};

			VertexDeclaration = new VertexDeclaration(elements) { Name = "TerrainVertex.VertexDeclaration" };
		}

		public TerrainVertex(Vector4b position, Color color, HalfVector2 textureCoordinate)
		{
			this.Position = position;
			this.Color = color;
			this.TextureCoordinate = textureCoordinate;
		}

		public static bool operator ==(TerrainVertex left, TerrainVertex right)
		{
			return (((left.Position == right.Position) && (left.Color == right.Color)) && (left.TextureCoordinate == right.TextureCoordinate));
		}

		public static bool operator !=(TerrainVertex left, TerrainVertex right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj.GetType() != base.GetType())
			{
				return false;
			}

			return (this == ((TerrainVertex)obj));
		}

		//public override int GetHashCode()
		//{
		//    return Position.X ^ Position.Y ^ Position.Z ^ Position.W;
		//}

		public override string ToString()
		{
			return String.Format("Position: {0}, TextureCoordinate: {1}, Color: {2}", Position, TextureCoordinate, Color);
		}
	}
}