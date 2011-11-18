using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Framework.Core.Diagnostics.Logging;
using Framework.Core.Packages;

namespace Game.GamePackages
{
	public class GamePackageManager
	{
		private Settings settings;
		public IGamePackage GamePackage;

		public void Initialize()
		{
			Logger.Log<GamePackageManager>(LogLevel.Info, "Initialize");

			try
			{
				GamePackage = null;

				settings = GetSettings();

				var settingsGamePackage = GetDefaultGamePackage();
				var assembly = Assembly.LoadFrom(settingsGamePackage.Assembly);

				// Try to locate the main class which inherits from IGamePackage within the assembly
				Type type = typeof(IGamePackage);
				var gamePackageType = assembly.GetTypes().ToList().SingleOrDefault(p => type.IsAssignableFrom(p));

				// We successfully located the class
				if (gamePackageType != null)
				{
					GamePackage = (IGamePackage)Activator.CreateInstance(gamePackageType);
				}
				// No class was found that inherits from IGamePackage
				else
				{
					Logger.Log<GamePackageManager>(LogLevel.Info, "Initialize failed. No instance of IGamePackage found in assembly {0}", settingsGamePackage.Assembly);
				}
			}
			catch (Exception e)
			{
				Logger.Log<GamePackageManager>(LogLevel.Fatal, e.Message);
			}
		}

		private Settings GetSettings()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Settings));
			var settings = (Settings)serializer.Deserialize(File.OpenRead(@"GamePackages\Settings.xml"));

			// Set all assembly paths relative to the executing assembly
			foreach (var gamePackage in settings.GamePackages)
			{
				gamePackage.Assembly = gamePackage.Assembly.Insert(0, @"GamePackages\").Replace(@"\\", @"\");
			}

			return settings;
		}

		private GamePackageInfo GetDefaultGamePackage()
		{
			if (settings == null ||
				settings.GamePackages == null ||
				settings.GamePackages.Count == 0)
			{
				return null;
			}

			return settings.GamePackages[0];
		}
	}
}