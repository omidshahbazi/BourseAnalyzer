using GameFramework.ASCIISerializer;
using GameFramework.DatabaseManaged;
using System.IO;

namespace Core
{
	public struct DataUpdaterConfig
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
		public int CalclationCount;
		public float LowRSI;
		public float MidRSI;
		public float HighRSI;
	}

	public struct DataAnalyzerConfig
	{
		public float WorkHour;

		public TrendLineConfig TrendLine;
		public RelativeStrengthIndexConfig RelativeStrengthIndex;
	}

	public struct AnalyzeValidatorConfig
	{
		public float WorkHour;
	}

	public struct AnalyzeReporterConfig
	{
		public float WorkHour;

		public string Host;
		public ushort Port;
		public string Username;
		public string Password;
	}

	public struct Config
	{
		public Database.CreateInfo DatabaseConnection;

		public int CheckSchedulesPeriod;

		public DataUpdaterConfig DataUpdater;
		public DataAnalyzerConfig DataAnalyzer;
		public AnalyzeValidatorConfig AnalyzeValidator;
		public AnalyzeReporterConfig AnalyzeReporter;
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
