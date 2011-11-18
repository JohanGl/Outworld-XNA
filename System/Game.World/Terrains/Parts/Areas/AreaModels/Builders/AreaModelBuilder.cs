using System;
using System.Collections.Generic;
using Game.World.Terrains.Parts.Tiles;
using Game.World.Terrains.Rendering;
using Game.World.Terrains.Rendering.MeshPools;
using Game.World.Terrains.Visibility;

namespace Game.World.Terrains.Parts.Areas.AreaModels.Builders
{
	public class AreaModelBuilder
	{
		private TerrainVisibility terrainVisibility;
		private AreaModelBuildInfo buildInfo;
		private BlockMeshBuilder blockBuilder;
		private TerrainMeshBuildInfo[] meshLookup;
		private readonly int totalTileTypes;
		private Area area;

		public AreaModelBuilder(TerrainVisibility terrainVisibility)
		{
			this.terrainVisibility = terrainVisibility;
			buildInfo = new AreaModelBuildInfo();
			blockBuilder = new BlockMeshBuilder();

			totalTileTypes = Enum.GetValues(typeof(TileType)).Length;

			InitializeMeshLookup();
		}

		private void InitializeMeshLookup()
		{
			meshLookup = new TerrainMeshBuildInfo[totalTileTypes];

			for (int i = 0; i < totalTileTypes; i++)
			{
				int maxVertices = (int)((Area.TotalTiles * 0.5f) * 24);
				int maxIndices = (int)(maxVertices * 1.5d);

				var mesh = new TerrainMeshBuildInfo
				{
					Vertices = new TerrainVertex[maxVertices],
					Indices = new int[maxIndices]
				};
				
				meshLookup[i] = mesh;
			}
		}

		private void ClearMeshLookup()
		{
			// Reset all tile types
			for (int i = 0; i < totalTileTypes; i++)
			{
				meshLookup[i].VertexCount = 0;
				meshLookup[i].IndexCount = 0;
			}
		}

		public void Build(Area area)
		{
			Build(area, terrainVisibility.GetAreaNeighbors(area));
		}

		public void Build(Area area, List<Area> neighbors)
		{
			this.area = area;

			// Clear any previous model data before building the model
			area.Model.Clear();

			// Skip empty models
			if (area.Info.IsEmpty)
			{
				return;
			}

			ClearMeshLookup();

			buildInfo.Initialize(area, neighbors);

			blockBuilder.Initialize(ref buildInfo, ref meshLookup);

			area.Model.TotalFaces = Area.TotalTiles * 12;
			area.Model.TotalVisibleFaces = 0;

			// Down/up (vertically)
			for (int y = 0; y < Area.Size.Y; y++)
			{
				int currentY = Area.LevelSize * y;
				int tileY = Area.Size.Y - (y + 1);

				// In/out
				for (int z = 0; z < Area.Size.Z; z++)
				{
					int currentZ = z * Area.Size.X;

					// Left/right
					for (int x = 0; x < Area.Size.X; x++)
					{
						int itemIndex = currentY + currentZ + x;

						// Skip empty blocks
						if (area.Info.Tiles[itemIndex].Type == TileType.Empty)
						{
							continue;
						}

						// Update the visiblility state of the current tile
						UpdateTileVisibleSides(x, y, z, itemIndex);

						// If no sides of the current tile are visible
						if (!buildInfo.TileVisibleSides[0] &&
							!buildInfo.TileVisibleSides[1] &&
							!buildInfo.TileVisibleSides[2] &&
							!buildInfo.TileVisibleSides[3] &&
							!buildInfo.TileVisibleSides[4] &&
							!buildInfo.TileVisibleSides[5])
						{
							// Skip this tile
							continue;
						}

						// Render the current tile
						blockBuilder.Render((int)area.Info.Tiles[itemIndex].Type, x, tileY, z);

						for (int i = 0; i < 6; i++)
						{
							if (buildInfo.TileVisibleSides[i])
							{
								area.Model.TotalVisibleFaces += 2;
							}
						}
					}
				}
			}

			BuildMesh();
		}

		private void UpdateTileVisibleSides(int x, int y, int z, int tileIndex)
		{
			if (y == 0)
			{
				// Check if we have any neighbor and if its TileType is empty
				if (buildInfo.AreaNeighbors[(int)TileSide.Top] != null)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Top] = buildInfo.AreaNeighbors[(int)TileSide.Top].Info.Tiles[tileIndex + (Area.LevelSize * (Area.Size.Y - 1))].Type == TileType.Empty;
				}
				// Check if we are at the outer edge
				else if (area.Info.Location.Y >= buildInfo.AreaLocationMax.Y)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Top] = false;
				}

				// The opposite neighbor of the previous check is guaranteed to exist. Check if its TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Bottom] = area.Info.Tiles[tileIndex + Area.LevelSize].Type == TileType.Empty;
			}
			else if (y == Area.Size.Y - 1)
			{
				// Check if we have any neighbor and if its TileType is empty
				if (buildInfo.AreaNeighbors[(int)TileSide.Bottom] != null)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Bottom] = buildInfo.AreaNeighbors[(int)TileSide.Bottom].Info.Tiles[tileIndex - (Area.LevelSize * (Area.Size.Y - 1))].Type == TileType.Empty;
				}
				// Check if we are at the outer edge
				else if (area.Info.Location.Y <= buildInfo.AreaLocationMin.Y)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Bottom] = false;
				}

				// The opposite neighbor of the previous check is guaranteed to exist. Check if its TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Top] = area.Info.Tiles[tileIndex - Area.LevelSize].Type == TileType.Empty;
			}
			else
			{
				// Both neighbors are guaranteed to exist. Check if their TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Top] = area.Info.Tiles[tileIndex - Area.LevelSize].Type == TileType.Empty;
				buildInfo.TileVisibleSides[(int)TileSide.Bottom] = area.Info.Tiles[tileIndex + Area.LevelSize].Type == TileType.Empty;
			}

			if (x == 0)
			{
				// Check if we have any neighbor and if its TileType is empty
				if (buildInfo.AreaNeighbors[(int)TileSide.Left] != null)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Left] = buildInfo.AreaNeighbors[(int)TileSide.Left].Info.Tiles[tileIndex + (Area.Size.X - 1)].Type == TileType.Empty;
				}
				// Check if we are at the outer edge
				else if (area.Info.Location.X <= buildInfo.AreaLocationMin.X)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Left] = false;
				}

				// The opposite neighbor of the previous check is guaranteed to exist. Check if its TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Right] = area.Info.Tiles[tileIndex + 1].Type == TileType.Empty;
			}
			else if (x == Area.Size.X - 1)
			{
				// Check if we have any neighbor and if its TileType is empty
				if (buildInfo.AreaNeighbors[(int)TileSide.Right] != null)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Right] = buildInfo.AreaNeighbors[(int)TileSide.Right].Info.Tiles[tileIndex - (Area.Size.X - 1)].Type == TileType.Empty;
				}
				// Check if we are at the outer edge
				else if (area.Info.Location.X >= buildInfo.AreaLocationMax.X)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Right] = false;
				}

				// The opposite neighbor of the previous check is guaranteed to exist. Check if its TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Left] = area.Info.Tiles[tileIndex - 1].Type == TileType.Empty;
			}
			else
			{
				// Both neighbors are guaranteed to exist. Check if their TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Left] = area.Info.Tiles[tileIndex - 1].Type == TileType.Empty;
				buildInfo.TileVisibleSides[(int)TileSide.Right] = area.Info.Tiles[tileIndex + 1].Type == TileType.Empty;
			}

			if (z == 0)
			{
				// Check if we have any neighbor and if its TileType is empty
				if (buildInfo.AreaNeighbors[(int)TileSide.Back] != null)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Back] = buildInfo.AreaNeighbors[(int)TileSide.Back].Info.Tiles[tileIndex + (Area.Size.X * (Area.Size.Z - 1))].Type == TileType.Empty;
				}
				// Check if we are at the outer edge
				else if (area.Info.Location.Z <= buildInfo.AreaLocationMin.Z)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Back] = false;
				}

				// The opposite neighbor of the previous check is guaranteed to exist. Check if its TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Front] = area.Info.Tiles[tileIndex + Area.Size.Z].Type == TileType.Empty;
			}
			else if (z == Area.Size.Z - 1)
			{
				// Check if we have any neighbor and if its TileType is empty
				if (buildInfo.AreaNeighbors[(int)TileSide.Front] != null)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Front] = buildInfo.AreaNeighbors[(int)TileSide.Front].Info.Tiles[tileIndex - (Area.Size.X * (Area.Size.Z - 1))].Type == TileType.Empty;
				}
				// Check if we are at the outer edge
				else if (area.Info.Location.Z >= buildInfo.AreaLocationMax.Z)
				{
					buildInfo.TileVisibleSides[(int)TileSide.Front] = false;
				}

				// The opposite neighbor of the previous check is guaranteed to exist. Check if its TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Back] = area.Info.Tiles[tileIndex - Area.Size.Z].Type == TileType.Empty;
			}
			else
			{
				// Both neighbors are guaranteed to exist. Check if their TileType is empty
				buildInfo.TileVisibleSides[(int)TileSide.Front] = area.Info.Tiles[tileIndex + Area.Size.Z].Type == TileType.Empty;
				buildInfo.TileVisibleSides[(int)TileSide.Back] = area.Info.Tiles[tileIndex - Area.Size.Z].Type == TileType.Empty;
			}
		}

		private void BuildMesh()
		{
			int terrainMeshCount = 0;

			// Build the vertex and index buffers
			for (int i = 0; i < totalTileTypes; i++)
			{
				var mesh = meshLookup[i];

				// Skip empty meshes
				if (mesh.VertexCount == 0)
				{
					continue;
				}

				// Get a terrain mesh from the pool
				var freeMesh = TerrainMeshPool.GetFreeItem(mesh.VertexCount);

				// Initialize the vertices
				freeMesh.VertexBuffer.SetData(mesh.Vertices, 0, mesh.VertexCount);
				freeMesh.VertexCount = mesh.VertexCount;

				// Initialize the indices
				freeMesh.IndexBuffer.SetData(mesh.Indices, 0, mesh.IndexCount);
				freeMesh.IndexCount = mesh.IndexCount;

				// Add the new mesh to the actual meshes dictionary
				area.Model.Meshes.Add((TileType)i, freeMesh);

				terrainMeshCount++;
			}

			// Set the empty state of the area
			area.Info.IsEmpty = (terrainMeshCount == 0);
		}
	}
}