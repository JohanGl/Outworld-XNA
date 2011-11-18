using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Framework.Core.Contexts
{
	public class ResourceContext
	{
		public ContentManager Content;
		public Dictionary<string, Texture2D> Textures;
		public Dictionary<string, SpriteFont> Fonts;
		public Dictionary<string, Model> Models;
		public Dictionary<string, SoundEffect> Sounds;

		public ResourceContext()
		{
			Textures = new Dictionary<string, Texture2D>();
			Models = new Dictionary<string, Model>();
			Sounds = new Dictionary<string, SoundEffect>();
			Fonts = new Dictionary<string, SpriteFont>();
		}

		public void Clear(bool removeResourcesWithGlobalPrefix = false)
		{
			// Textures
			var keys = new List<string>(Textures.Keys);
			keys = Filter(keys, removeResourcesWithGlobalPrefix);
			keys.ForEach(p => Textures.Remove(p));

			// Models
			keys = new List<string>(Models.Keys);
			keys = Filter(keys, removeResourcesWithGlobalPrefix);
			keys.ForEach(p => Models.Remove(p));

			// Sounds
			keys = new List<string>(Sounds.Keys);
			keys = Filter(keys, removeResourcesWithGlobalPrefix);
			keys.ForEach(p => Sounds.Remove(p));

			// Fonts
			keys = new List<string>(Fonts.Keys);
			keys = Filter(keys, removeResourcesWithGlobalPrefix);
			keys.ForEach(p => Fonts.Remove(p));
		}

		private List<string> Filter(List<string> keys, bool removeResourcesWithGlobalPrefix)
		{
			if (removeResourcesWithGlobalPrefix)
			{
				var filteredKeys = new List<string>();

				foreach (var key in keys)
				{
					if (key.StartsWith("Global."))
					{
						continue;
					}

					filteredKeys.Add(key);
				}

				return filteredKeys;
			}

			return keys;
		}
	}
}