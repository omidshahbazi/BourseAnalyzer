﻿using GameFramework.ASCIISerializer;
using GameFramework.DatabaseManaged;
using System.IO;

namespace Core
{
	public class DataUpdaterConfig
	{
		public bool Enabled = true;
		public float WorkHour = 10.5F;
	}

	public class AnalyzeValidatorConfig
	{
		public bool Enabled = true;
		public float WorkHour = 10.75F;
	}

	public class RelativeStrengthIndexConfig
	{
		public bool Enabled = false;
		public int HistoryCount = 11;
		public int CalculationCount = 90;
		public float LowRSI = 0.3F;
		public float MidRSI = 0.5F;
		public float HighRSI = 0.7F;
		public float MaxRSI = 1;
		public float IgnoreThreshold = 0.05F;
	}

	public class MovingAverageConvergenceDivergenceConfig
	{
		public bool Enabled = true;
		public int SlowHistoryCount = 26;
		public int FastHistoryCount = 12;
		public int SignalHistoryCount = 9;
		public int CalculationCount = 90;
		public int PostPeriodCount = 3;
		public float IgnoreThreshold = 0.003F;
	}

	public class SimpleMovingAverageConfig
	{
		public bool Enabled = false;
		public int SlowHistoryCount = 31;
		public int FastHistoryCount = 14;
		public int CalculationCount = 90;
	}

	public class AwesomeOscillatoreConfig
	{
		public bool Enabled = false;
		public int SlowHistoryCount = 5;
		public int FastHistoryCount = 34;
		public int CalculationCount = 90;
	}

	public class DataAnalyzerConfig
	{
		public bool Enabled = true;
		public float WorkHour = 11;
		public int MinimumTradeCount = 10;
		public int BacklogCount = 5;
		public int SignalConfirmationCount = 2;

		public bool WriteToFile = true;
		public string Path = "Output/Analyzes/";

		public RelativeStrengthIndexConfig RelativeStrengthIndex = new RelativeStrengthIndexConfig();
		public MovingAverageConvergenceDivergenceConfig MovingAverageConvergenceDivergence = new MovingAverageConvergenceDivergenceConfig();
		public SimpleMovingAverageConfig SimpleMovingAverage = new SimpleMovingAverageConfig();
		public AwesomeOscillatoreConfig AwesomeOscillatore = new AwesomeOscillatoreConfig();
	}

	public class AnalyzeReporterConfig
	{
		public bool Enabled = true;
		public float WorkHour = 11.5F;

		public string Host;
		public ushort Port;
		public string Username;
		public string Password;

		public bool WriteToFile = false;
		public string Path = "Output/AnalyzeReport/";
	}

	public class ValidationReporterConfig
	{
		public bool Enabled = true;
		public float WorkHour = 11.5F;

		public string Host;
		public ushort Port;
		public string Username;
		public string Password;

		public bool WriteToFile = false;
		public string Path = "Output/ValidateReport/";
	}

	public class BackupMakerConfig
	{
		public bool Enabled = true;
		public float WorkHour = 11.5F;

		public string MySQLDumpPath = "C:/Program Files/MySQL/MySQL Server 8.0/bin";
		public string OutputPath = "Backup/";
	}

	public class Config
	{
		public Database.CreateInfo DatabaseConnection;

		public int CheckSchedulesPeriod = 60;

		public DataUpdaterConfig DataUpdater = new DataUpdaterConfig();
		public AnalyzeValidatorConfig AnalyzeValidator = new AnalyzeValidatorConfig();
		public DataAnalyzerConfig DataAnalyzer = new DataAnalyzerConfig();
		public AnalyzeReporterConfig AnalyzeReporter = new AnalyzeReporterConfig();
		public ValidationReporterConfig ValidationReporter = new ValidationReporterConfig();
		public BackupMakerConfig BackupMaker = new BackupMakerConfig();
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