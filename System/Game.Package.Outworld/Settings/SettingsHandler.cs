using Framework.Core.Services;
using Outworld.Settings.Global;

namespace Outworld.Settings
{
	public class SettingsHandler
	{
		public GlobalSettings GetGlobalSettings()
		{
			var settings = ServiceLocator.Get<GlobalSettings>();

			if (settings == null)
			{
				settings = new GlobalSettings();
			}

			return settings;
		}

		//public void Initialize()
		//{
			//GlobalSettings = new GlobalSettings();

			//XmlSerializer serializer = new XmlSerializer(typeof(GlobalSettings));
			//StringWriter stringWriter = new StringWriter();
			//serializer.Serialize(stringWriter, GlobalSettings);
			//string test = stringWriter.ToString();
		//}
	}
}