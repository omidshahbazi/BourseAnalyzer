using GameFramework.ASCIISerializer;
using GameFramework.DatabaseManaged;
using System.IO;

namespace Core
{
	public struct UpdaterConfig
	{
		public float WorkHour;
	}

	public struct TrendLineConfig
	{
		public int LongTermSeconds;
		public float ShortTermInfiltrate;
		public float LongTermInfiltrate;
	}

	public struct RelativeStrengthIndexConfig
	{
		public int MaxHistoryCount;
		public float LowRSI;
		public float HighRSI;
	}

	public struct AnalyzerConfig
	{
		public float WorkHour;

		public TrendLineConfig TrendLine;
		public RelativeStrengthIndexConfig RelativeStrengthIndex;
	}

	public struct Config
	{
		public Database.CreateInfo DatabaseConnection;

		public UpdaterConfig Updater;
		public AnalyzerConfig Analyzer;
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
