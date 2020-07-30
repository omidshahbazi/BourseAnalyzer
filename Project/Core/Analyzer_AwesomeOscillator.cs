using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Diagnostics;

namespace Core
{
	public static partial class Analyzer
	{
		public static class AwesomeOscillator
		{
			//https://currency.com/awesome-oscillator-vs-macd

			private static int SlowHistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.AwesomeOscillatore.SlowHistoryCount; }
			}

			private static int FastHistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.AwesomeOscillatore.FastHistoryCount; }
			}

			private static int CalculationCount
			{
				get { return ConfigManager.Config.DataAnalyzer.AwesomeOscillatore.CalculationCount; }
			}

			public static Result Analyze(Info Info)
			{
				if (!ConfigManager.Config.DataAnalyzer.AwesomeOscillatore.Enabled)
					return null;

				DataTable data = Info.HistoryData;

				DataTable chartData = GenerateData(data);
				if (chartData == null)
					return null;

				Result result = new Result() { Signals = new Signal[ConfigManager.Config.DataAnalyzer.BacklogCount], Data = chartData };

				for (int i = 0; i < result.Signals.Length; ++i)
				{
					int index = chartData.Rows.Count - 1 - i;

					double prevAO = Convert.ToDouble(chartData.Rows[index - 1]["ao"]);
					double currAO = Convert.ToDouble(chartData.Rows[index]["ao"]);

					int action = 0;
					double worthiness = 0;
					Analyzer.CheckPointCrossover(prevAO, currAO, 0, out action);

					Debug.Assert(false, "Calculate worthiness like MACD");

					result.Signals[result.Signals.Length - 1 - i] = new Signal() { Action = action, Worthiness = worthiness };
				}

				return result;
			}

			private static DataTable GenerateData(DataTable Data)
			{
				if (FastHistoryCount <= 0)
				{
					ConsoleHelper.WriteError("FastHistoryCount must be grater than 0, current value is {0}", FastHistoryCount);
					return null;
				}

				if (SlowHistoryCount <= 0)
				{
					ConsoleHelper.WriteError("SlowHistoryCount must be grater than 0, current value is {0}", SlowHistoryCount);
					return null;
				}

				if (SlowHistoryCount < FastHistoryCount)
				{
					ConsoleHelper.WriteError("SlowHistoryCount must be grater than FastHistoryCount, current values area {0}, {1}", SlowHistoryCount, FastHistoryCount);
					return null;
				}

				if (CalculationCount < ConfigManager.Config.DataAnalyzer.BacklogCount + 1)
				{
					ConsoleHelper.WriteError("CalculationCount must be grater than {0}, current value is {1}", ConfigManager.Config.DataAnalyzer.BacklogCount, CalculationCount);
					return null;
				}

				int calculationCount = Math.Min(Math.Max(ConfigManager.Config.DataAnalyzer.BacklogCount + 1, Data.Rows.Count - SlowHistoryCount), CalculationCount);

				DataTable chartData = new DataTable();
				chartData.Columns.Add("ao", typeof(double));

				DataTable slowSMAData = Analyzer.GenerateSimpleMovingAverageData(Data, "median", SlowHistoryCount, calculationCount);
				DataTable fastSMAData = Analyzer.GenerateSimpleMovingAverageData(Data, "median", FastHistoryCount, calculationCount);

				if (slowSMAData == null || fastSMAData == null)
					return null;

				for (int i = 0; i < calculationCount; ++i)
					chartData.Rows.Add(Convert.ToDouble(fastSMAData.Rows[i]["sma"]) - Convert.ToDouble(slowSMAData.Rows[i]["sma"]));

				return chartData;
			}
		}
	}
}