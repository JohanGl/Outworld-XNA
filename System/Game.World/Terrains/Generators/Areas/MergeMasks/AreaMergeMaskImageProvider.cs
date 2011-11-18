using System.Collections.Generic;
using Framework.Core.Contexts;
using Framework.Core.Services;
using Game.World.Terrains.Generators.Noise.Managers.Areas;
using Game.World.Terrains.Parts.Areas;
using Microsoft.Xna.Framework;

namespace Game.World.Terrains.Generators.Areas.MergeMasks
{
	public class AreaMergeMaskImageProvider
	{
		private List<byte[]> masks;
		private Dictionary<string, int> patterns;
		private char[] neighborMap;
		private string pattern;
		private const string EmptyPattern = "0000-0000";
		private const int startIndexCorners = 15;

		public AreaMergeMaskImageProvider()
		{
			masks = new List<byte[]>();
			patterns = new Dictionary<string, int>();
			neighborMap = new char[9];

			InitializeMergeMaskCutOuts();
			InitializePatterns();
		}

		public byte[] GetMergeMask(NoiseAreaType[] areaTypes, NoiseAreaType neighborTypeToBaseMaskOn)
		{
			// Create the pattern for the neighbor type
			pattern = GetPattern(areaTypes, neighborTypeToBaseMaskOn);

			if (pattern == EmptyPattern)
			{
				return null;
			}

			// Return the mask that matches the pattern
			return masks[patterns[pattern]];
		}

		private string GetPattern(NoiseAreaType[] areaTypes, NoiseAreaType neighborTypeToBaseMaskOn)
		{
			// Clear the neighbor map to a default state
			neighborMap[0] = '0';
			neighborMap[1] = '0';
			neighborMap[2] = '0';
			neighborMap[3] = '0';
			neighborMap[4] = '-';
			neighborMap[5] = '0';
			neighborMap[6] = '0';
			neighborMap[7] = '0';
			neighborMap[8] = '0';

			// Sides: Left, Top, Right, Bottom
			if (areaTypes[3] == neighborTypeToBaseMaskOn) neighborMap[0] = '1';
			if (areaTypes[1] == neighborTypeToBaseMaskOn) neighborMap[1] = '1';
			if (areaTypes[5] == neighborTypeToBaseMaskOn) neighborMap[2] = '1';
			if (areaTypes[7] == neighborTypeToBaseMaskOn) neighborMap[3] = '1';

			// Corners: Top Left, Top Right, Bottom Right, Bottom Left
			if (areaTypes[0] == neighborTypeToBaseMaskOn) neighborMap[5] = '1';
			if (areaTypes[2] == neighborTypeToBaseMaskOn) neighborMap[6] = '1';
			if (areaTypes[8] == neighborTypeToBaseMaskOn) neighborMap[7] = '1';
			if (areaTypes[6] == neighborTypeToBaseMaskOn) neighborMap[8] = '1';

			// Return the pattern as a string
			return new string(neighborMap);
		}

		private void InitializeMergeMaskCutOuts()
		{
			// Get the terrain merge mask texture
			var texture = ServiceLocator.Get<GameContext>().Resources.Textures["Global.TerrainMergeMask"];

			// Extract the pixels from the texture
			var textureData = new Color[texture.Width * texture.Height];
			texture.GetData(textureData);

			// Create cutouts from the texture pixels
			for (int y = 0; y < 2; y++)
			{
				for (int x = 0; x < startIndexCorners; x++)
				{
					masks.Add(GetMergeMaskCutOut(textureData, texture.Width, x, y));
				}
			}
		}

		private byte[] GetMergeMaskCutOut(Color[] textureData, int textureWidth, int cellX, int cellY)
		{
			var result = new byte[Area.LevelSize];
			int index = 0;

			// Translate the cell coordinates into texture coordinates
			int x = (Area.Size.X * cellX) + (cellX * 1);
			int y = (Area.Size.Z * cellY) + (cellY * 1);

			// Get all pixels at the current coordinates
			for (int currentY = y; currentY < y + Area.Size.Z; currentY++)
			{
				for (int currentX = x; currentX < x + Area.Size.X; currentX++)
				{
					result[index++] = textureData[(textureWidth * currentY) + currentX].R;
				}
			}

			return result;
		}

		private void InitializePatterns()
		{
			// All possible combinations
			var combinations = new string[] { "1000", "0100", "0010", "0001", "1100", "0110", "0011", "1001", "1010", "0101", "1101", "1110", "0111", "1011", "1111" };

			// Pattern combinations for all sides
			for (int i = 0; i < combinations.Length; i++)
			{
				patterns.Add(combinations[i] + "-0000", i);
			}

			// Pattern combinations for all corners
			for (int i = 0; i < combinations.Length; i++)
			{
				patterns.Add("0000-" + combinations[i], i + startIndexCorners);
			}

			// Pattern combinations for both sides and corners in all possible combinations
			int index = patterns.Count;
			for (int i = 0; i < combinations.Length; i++)
			{
				for (int j = 0; j < combinations.Length; j++)
				{
					patterns.Add(combinations[i] + "-" + combinations[j], index++);
					masks.Add(CreateMergeMask(i, j));
				}
			}
		}

		private byte[] CreateMergeMask(int sideIndex, int cornerIndex)
		{
			var result = new byte[Area.LevelSize];

			// Loop through all pixels
			for (int i = 0; i < Area.LevelSize; i++)
			{
				// Store the brightest pixel of the two texture masks
				result[i] = (masks[sideIndex][i] >= masks[startIndexCorners + cornerIndex][i]) ? masks[sideIndex][i] : masks[startIndexCorners + cornerIndex][i];
			}

			return result;
		}
	}
}