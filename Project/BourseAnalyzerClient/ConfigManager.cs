using GameFramework.ASCIISerializer;
using System.IO;

namespace BourseAnalyzerClient
{
	class Config
	{
		public string Host = "37.152.185.126";
		public ushort Port = 5000;

		public string LastUsername;
	}

	static class ConfigManager
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
				Config = new Config();

				Save();

				return;
			}

			Config = Creator.Bind<Config>(Creator.Create<ISerializeObject>(File.ReadAllText(path)));
		}

		public static void Save()
		{
			File.WriteAllText(Path.GetFullPath(FILE_NAME), Creator.Serialize<ISerializeObject>(Config).Content);
		}
	}
}
