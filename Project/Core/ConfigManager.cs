using GameFramework.ASCIISerializer;
using GameFramework.DatabaseManaged;
using System.IO;

namespace Core
{
	public class DataUpdaterConfig
	{
		public bool Enabled = true;
		public float WorkHour = 13;
	}

	public class RelativeStrengthIndexConfig
	{
		public bool Enabled = true;
		public int HistoryCount = 14;
		public int CalculationCount = 90;
		public float LowRSI = 0.3F;
		public float MidRSI = 0.5F;
		public float HighRSI = 0.7F;
		public float MaxRSI = 1;

		public bool WriteToCSV = false;
		public string CSVPath = "Output/RSI/";
	}

	public class MovingAverageConvergenceDivergenceConfig
	{
		public bool Enabled = true;
		public int SlowHistoryCount = 26;
		public int FastHistoryCount = 12;
		public int SignalHistoryCount = 9;
		public int CalculationCount = 90;

		public bool WriteToCSV = false;
		public string CSVPath = "Output/MACD/";
	}

	public class SimpleMovingAverageConfig
	{
		public bool Enabled = true;
		public int[] HistoryCount = new int[] { 14, 31 };
		public int FastHistoryCount = 40;
		public int CalculationCount = 90;

		public bool WriteToCSV = false;
		public string CSVPath = "Output/MACD/";
	}

	public class DataAnalyzerConfig
	{
		public bool Enabled = true;
		public float WorkHour = 13;

		public RelativeStrengthIndexConfig RelativeStrengthIndex = new RelativeStrengthIndexConfig();
		public MovingAverageConvergenceDivergenceConfig MovingAverageConvergenceDivergence = new MovingAverageConvergenceDivergenceConfig();
		public SimpleMovingAverageConfig SimpleMovingAverage = new SimpleMovingAverageConfig();
	}

	public class AnalyzeValidatorConfig
	{
		public bool Enabled = true;
		public float WorkHour = 13;
	}

	public class AnalyzeReporterConfig
	{
		public bool Enabled = true;
		public float WorkHour = 13;

		public string Host;
		public ushort Port;
		public string Username;
		public string Password;
	}

	public class ValidationReporterConfig
	{
		public bool Enabled = true;
		public float WorkHour = 13;

		public string Host;
		public ushort Port;
		public string Username;
		public string Password;
	}

	public class Config
	{
		public Database.CreateInfo DatabaseConnection;

		public int CheckSchedulesPeriod = 60;

		public DataUpdaterConfig DataUpdater = new DataUpdaterConfig();
		public AnalyzeValidatorConfig AnalyzeValidator = new AnalyzeValidatorConfig();
		public DataAnalyzerConfig DataAnalyzer = new DataAnalyzerConfig();
		public ValidationReporterConfig ValidationReporter = new ValidationReporterConfig();
		public AnalyzeReporterConfig AnalyzeReporter = new AnalyzeReporterConfig();
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
				Config = new Config();

				File.WriteAllText(path, Creator.Serialize<ISerializeObject>(Config).Content);
				return;
			}

			Config = Creator.Bind<Config>(Creator.Create<ISerializeObject>(File.ReadAllText(path)));
		}
	}
}