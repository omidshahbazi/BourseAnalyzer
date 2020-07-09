using GameFramework.ASCIISerializer;
using GameFramework.DatabaseManaged;
using System;
using System.IO;

namespace Core
{
	public struct Config
	{
		public Database.CreateInfo DatabaseConnection;
	}

	public static class ConfigManager
	{
		public const string FILE_NAME = "Configurations.json";

		public static Config Config
		{
			get;
			private set;
		}

		static ConfigManager()
		{
			string path = Path.GetFullPath(FILE_NAME);
			if (!File.Exists(path))
			{
				File.WriteAllText(path, Creator.Serialize<ISerializeObject>(Config).Content);
				return;
			}

			Config = Creator.Bind<Config>(Creator.Create<ISerializeObject>(File.ReadAllText(path)));
		}
	}
}
