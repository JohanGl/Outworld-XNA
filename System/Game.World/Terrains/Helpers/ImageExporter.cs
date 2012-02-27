using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using Framework.Core.Common;
using Game.World.Terrains.Generators.Noise;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Parts.Areas;
using Game.World.Terrains.Parts.Tiles;

namespace Game.World.Terrains.Helpers
{
	public class ImageExporter
	{
		public static void SaveImage(string fileName, Microsoft.Xna.Framework.Color[] data, int width, int height)
        {
            using (var bitmap = new Bitmap(width, height))
            {
				int index = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        bitmap.SetPixel(x, y, Color.FromArgb(data[index].A, data[index].R, data[index].G, data[index].B));
						index++;
                    }
                }

                bitmap.Save(fileName);
            }
        }

		public static void SaveGrayScaleImage(string fileName, byte[] data, int width, int height)
		{
			using (var bitmap = new Bitmap(width, height))
			{
				int index = 0;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						bitmap.SetPixel(x, y, Color.FromArgb(255, data[index], data[index], data[index]));
						index++;
					}
				}

				bitmap.Save(fileName);
			}
		}

		public static void AreaToBitmap(string fileName, Area area, bool increaseContrast = false)
		{
			using (var bitmap = new Bitmap(Area.Size.X, Area.Size.Z))
			{
				int brightnessPerBlock = increaseContrast ? 7 : 1;
				int areaHeight = Area.Size.Y - 1;

				for (int z = 0; z < Area.Size.Z; z++)
				{
					for (int x = 0; x < Area.Size.X; x++)
					{
						int height = 0;

						for (int y = areaHeight; y >= 0; y--)
						{
							int index = (z * Area.Size.X) + (y * Area.LevelSize) + x;

							if (area.Info.Tiles[index].Type != TileType.Empty)
							{
								height = brightnessPerBlock * (areaHeight - y);
							}
						}

						bitmap.SetPixel(x, z, Color.FromArgb(255, height, height, height));
					}
				}

				bitmap.Save(fileName);
			}
		}

		public static void AreaTo3DBitmap(string fileName, Area area)
		{
			var tileTypeColors = GetTileTypes();

			int tileSizeInPixels = 1;

			int imageWidth = (tileSizeInPixels * Area.Size.X) + Area.Size.Z;
			int imageHeight = (tileSizeInPixels * Area.Size.Y) + Area.Size.Z;

			int offset = 0;

			using (var bitmap = new Bitmap(imageWidth, imageHeight))
			{
				for (int z = 0; z < Area.Size.Z; z++)
				{
					for (int x = 0; x < Area.Size.X; x++)
					{
						// Render the current column
						for (int y = 0; y < Area.Size.Y; y++)
						{
							int index = (z * Area.Size.X) + (Area.LevelSize * y) + x;

							var type = area.Info.Tiles[index].Type;

							if (type == TileType.Empty)
							{
								continue;
							}

							byte r = (byte)(Math.Max(0, tileTypeColors[type].R - (z * 3)));
							byte g = (byte)(Math.Max(0, tileTypeColors[type].G - (z * 3)));
							byte b = (byte)(Math.Max(0, tileTypeColors[type].B - (z * 3)));

							bitmap.SetPixel(x + offset, y + offset, Color.FromArgb(255, r, g, b));
						}
					}

					offset++;
				}

				bitmap.Save(fileName);
			}
		}

		private static Dictionary<TileType, Color> GetTileTypes()
		{
			var tileTypeColors = new Dictionary<TileType, Color>();
			tileTypeColors.Add(TileType.Grass, Color.FromArgb(255, 150, 150, 39));
			tileTypeColors.Add(TileType.Grass2, Color.FromArgb(255, 90, 164, 49));
			tileTypeColors.Add(TileType.Mud, Color.FromArgb(255, 182, 154, 106));
			tileTypeColors.Add(TileType.Sand, Color.FromArgb(255, 210, 206, 145));
			tileTypeColors.Add(TileType.Stone, Color.FromArgb(255, 190, 190, 190));
			tileTypeColors.Add(TileType.Stone2, Color.FromArgb(255, 170, 170, 170));
			return tileTypeColors;
		}

		public static void AreaSliceOnXAxisToBitmap(string fileName, Area area, int indexZ)
		{
			var tileTypeColors = GetTileTypes();

			int imageWidth = Area.Size.X;
			int imageHeight = Area.Size.Y;

			using (var bitmap = new Bitmap(imageWidth, imageHeight))
			{
				for (int x = 0; x < Area.Size.X; x++)
				{
					int z = indexZ;

					// Render the current column
					for (int y = 0; y < Area.Size.Y; y++)
					{
						int index = (z * Area.Size.X) + (Area.LevelSize * y) + x;

						var type = area.Info.Tiles[index].Type;

						if (type == TileType.Empty)
						{
							continue;
						}

						byte r = tileTypeColors[type].R;
						byte g = tileTypeColors[type].G;
						byte b = tileTypeColors[type].B;

						bitmap.SetPixel(x, y, Color.FromArgb(255, r, g, b));
					}
				}

				bitmap.Save(fileName);
			}
		}

		public static void AreaSliceOnZAxisToBitmap(string fileName, Area area, int indexX)
		{
			var tileTypeColors = GetTileTypes();

			int imageWidth = Area.Size.X;
			int imageHeight = Area.Size.Y;

			using (var bitmap = new Bitmap(imageWidth, imageHeight))
			{
				for (int z = 0; z < Area.Size.Z; z++)
				{
					int x = indexX;

					// Render the current column
					for (int y = 0; y < Area.Size.Y; y++)
					{
						int index = (z * Area.Size.X) + (Area.LevelSize * y) + x;

						var type = area.Info.Tiles[index].Type;

						if (type == TileType.Empty)
						{
							continue;
						}

						byte r = tileTypeColors[type].R;
						byte g = tileTypeColors[type].G;
						byte b = tileTypeColors[type].B;

						bitmap.SetPixel(z, y, Color.FromArgb(255, r, g, b));
					}
				}

				bitmap.Save(fileName);
			}
		}

		public static void AreaSliceOnYAxisToBitmap(string fileName, Area area, int indexX, int indexZ)
		{
			var tileTypeColors = GetTileTypes();

			int imageWidth = 1;
			int imageHeight = Area.Size.Y;

			using (var bitmap = new Bitmap(imageWidth, imageHeight))
			{
				// Render the current column
				for (int y = 0; y < Area.Size.Y; y++)
				{
					int index = (indexZ * Area.Size.X) + (Area.LevelSize * y) + indexX;

					var type = area.Info.Tiles[index].Type;

					if (type == TileType.Empty)
					{
						continue;
					}

					byte r = tileTypeColors[type].R;
					byte g = tileTypeColors[type].G;
					byte b = tileTypeColors[type].B;

					bitmap.SetPixel(0, y, Color.FromArgb(255, r, g, b));
				}

				bitmap.Save(fileName);
			}
		}

		public static void AreasToBitmap(string fileName, List<Area> areas, bool displayAreaGrid)
		{
			// Calculate the total size of the result bitmap
			int minX = (int)areas.Min(p => p.Info.BoundingBox.Min.X);
			int minY = (int)areas.Min(p => p.Info.BoundingBox.Min.Y);
			int minZ = (int)areas.Min(p => p.Info.BoundingBox.Min.Z);
			int bitmapWidth = (int)areas.Max(p => p.Info.BoundingBox.Max.X) - minX;
			int bitmapHeight = (int)areas.Max(p => p.Info.BoundingBox.Max.Z) - minZ;

			float totalDepth = areas.Max(p => p.Info.BoundingBox.Max.Y) - areas.Min(p => p.Info.BoundingBox.Min.Y);
			float depthStep = 255f / totalDepth;

			using (var bitmap = new Bitmap(bitmapWidth, bitmapHeight))
			{
				// Loop through all areas
				foreach (var area in areas.OrderBy(p => p.Info.Location.Y))
				{
					// Calculate the area offset within the bitmap space
					int offsetX = (int)area.Info.BoundingBox.Min.X - minX;
					int offsetY = (int)area.Info.BoundingBox.Min.Y - minY;
					int offsetZ = (int)area.Info.BoundingBox.Min.Z - minZ;

					// Render all pixels of the current area
					for (int z = 0; z < Area.Size.Z; z++)
					{
						for (int x = 0; x < Area.Size.X; x++)
						{
							int? level = null;

							// Find the first tile which is solid and calculate its light strength based on the area y location
							for (int y = 0; y < 16; y++)
							{
								var tile = area.Info.Tiles[x + (Area.LevelSize * y) + (Area.Size.Z * z)];

								if (tile.Type != TileType.Empty)
								{
									level = (int)(depthStep * (offsetY + (16 - y)));
									break;
								}
							}

							if (level.HasValue)
							{
								// Grid pattern
								if (displayAreaGrid && (x == 0 || z == 0))
								{
									level = (level + 20 > 255) ? 255 : level + 20;
								}

								bitmap.SetPixel(offsetX + x, offsetZ + z, Color.FromArgb(255, level.Value, level.Value, level.Value));
							}
						}
					}
				}

				bitmap.Save(fileName);
			}
		}

		public static void AreasToThemeBitmap(string fileName, List<Area> areas)
		{
			var areaThemeColors = new Dictionary<NoiseAreaType, Color>();
			areaThemeColors.Add(NoiseAreaType.Plains, Color.FromArgb(255, 90, 164, 49));
			areaThemeColors.Add(NoiseAreaType.Mountains, Color.FromArgb(255, 210, 200, 150));

			// Calculate the total size of the result bitmap
			int minX = (int)areas.Min(p => p.Info.BoundingBox.Min.X);
			int minZ = (int)areas.Min(p => p.Info.BoundingBox.Min.Z);
			int bitmapWidth = (int)areas.Max(p => p.Info.BoundingBox.Max.X) - minX;
			int bitmapHeight = (int)areas.Max(p => p.Info.BoundingBox.Max.Z) - minZ;

			using (var bitmap = new Bitmap(bitmapWidth, bitmapHeight))
			{
				// Loop through all areas
				foreach (var area in areas.OrderBy(p => p.Info.Location.Y))
				{
					// Calculate the area offset within the bitmap space
					int offsetX = (int)area.Info.BoundingBox.Min.X - minX;
					int offsetZ = (int)area.Info.BoundingBox.Min.Z - minZ;

					// Render all pixels of the current area
					for (int z = 0; z < Area.Size.Z; z++)
					{
						for (int x = 0; x < Area.Size.X; x++)
						{
							Color color = areaThemeColors[area.Info.Type];

							bitmap.SetPixel(offsetX + x, offsetZ + z, color);
						}
					}
				}

				bitmap.Save(fileName);
			}
		}

		public static void AreaToThemeAndMaskBitmap(string fileName, Area area, byte[] mask)
		{
			var areaThemeColors = new Dictionary<NoiseAreaType, Color>();
			areaThemeColors.Add(NoiseAreaType.Plains, Color.FromArgb(255, 90, 164, 49));
			areaThemeColors.Add(NoiseAreaType.Mountains, Color.FromArgb(255, 210, 200, 150));

			using (var bitmap = new Bitmap(Area.Size.X, Area.Size.Z))
			{
				for (int z = 0; z < Area.Size.Z; z++)
				{
					for (int x = 0; x < Area.Size.X; x++)
					{
						int maskColor = mask[(z * Area.Size.X) + x];

						if (maskColor > 0)
						{
							// Invert the color
							//maskColor = 255 - maskColor;

							float percentage = maskColor / 255f;

							//int r = (int)(areaThemeColors[area.Info.Type].R * percentage);
							//int g = (int)(areaThemeColors[area.Info.Type].G * percentage);
							//int b = (int)(areaThemeColors[area.Info.Type].B * percentage);

							int r = (int)(255 * percentage);
							int g = (int)(255 * percentage);
							int b = (int)(255 * percentage);

							bitmap.SetPixel(x, z, Color.FromArgb(255, r, g, b));
						}
						else
						{
							bitmap.SetPixel(x, z, areaThemeColors[area.Info.Type]);
						}

					}
				}

				bitmap.Save(fileName);
			}
		}

		public static void MergeBitmapsInFolderToBitmap(string fileName, string folderName, Vector2i min, Vector2i max)
		{
			var folder = new DirectoryInfo(folderName);
			var images = folder.GetFiles().Where(p => p.Extension == ".png").ToList();

			int subImageWidth = 3;
			int subImageHeight = 3;

			int bitmapWidth = 7 * subImageWidth;
			int bitmapHeight = 7 * subImageHeight;

			using (var bitmap = new Bitmap(bitmapWidth, bitmapHeight))
			{
				foreach (var image in images)
				{
					var tokens = image.Name.Replace(".png", "").Split(',');
					var coordinates = new Vector2i(int.Parse(tokens[0]), int.Parse(tokens[2]));

					int x = (coordinates.X * subImageWidth) + (3 * subImageWidth);
					int y = (coordinates.Y * subImageHeight) + (3 * subImageHeight);

					var imageData = (Bitmap)Image.FromFile(image.FullName);

					int imageX = 0;
					int imageY = 0;

					for (int currentY = y; currentY < y + subImageHeight; currentY++)
					{
						imageX = 0;

						for (int currentX = x; currentX < x + subImageWidth; currentX++)
						{
							bitmap.SetPixel(currentX, currentY, imageData.GetPixel(imageX, imageY));
							imageX++;
						}

						imageY++;
					}
				}

				bitmap.Save(fileName);
			}
		}

		public static void SaveNoiseToBitmap(string fileName, INoiseGenerator noiseGenerator, int width, int height)
		{
			var currentSize = noiseGenerator.GetOutputSize();

			// Generate the noise and save it to file
			noiseGenerator.SetOutputSize(width, height, 1);
			noiseGenerator.Generate(0, 0, 0);
			SaveGrayScaleImage(fileName, noiseGenerator.Output, width, height);

			// Reset the previous output size
			noiseGenerator.SetOutputSize(currentSize.X, currentSize.Y, currentSize.Z);
		}
	}
}