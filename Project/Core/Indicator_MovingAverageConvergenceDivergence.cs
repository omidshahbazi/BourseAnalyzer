using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public static partial class Indicator
	{
		public static class MovingAverageConvergenceDivergence
		{
			//https://commodity.com/technical-analysis/macd/
			//https://fairmontequities.com/how-to-calculate-the-macd/
			//https://www.wikihow.com/Read-MACDA
			//https://www.iexplain.org/ema-how-to-calculate/
			//https://www.brainyforex.com/macd-how-to-reduce-false-signals.html
			//https://www.daytrading.com/macd

			private static int SlowHistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.SlowHistoryCount; }
			}

			private static int FastHistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.FastHistoryCount; }
			}

			private static int SignalHistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.SignalHistoryCount; }
			}

			private static int CalculationCount
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.CalculationCount; }
			}

			public static DataTable Generate(Info Info)
			{
				if (!ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.Enabled)
					return null;

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

				if (CalculationCount < ConfigManager.Config.DataAnalyzer.BacklogCount)
				{
					ConsoleHelper.WriteError("CalculationCount must be grater than {0}, current value is {1}", ConfigManager.Config.DataAnalyzer.BacklogCount, CalculationCount);
					return null;
				}

				DataTable data = Info.HistoryData;

				int calculationCount = Math.Min(Math.Max(ConfigManager.Config.DataAnalyzer.BacklogCount + 1, data.Rows.Count - SlowHistoryCount - SignalHistoryCount), CalculationCount);

				int requiredCount = calculationCount + SlowHistoryCount + SignalHistoryCount;

				if (data.Rows.Count < requiredCount)
					return null;

				DataTable chartData = new DataTable();
				chartData.Columns.Add("macd", typeof(double));
				chartData.Columns.Add("signal", typeof(double));

				DataTable slowEMAData = Indicator.GenerateExponentialMovingAverage(data, "close", SlowHistoryCount, calculationCount + SignalHistoryCount);
				DataTable fastEMAData = Indicator.GenerateExponentialMovingAverage(data, "close", FastHistoryCount, calculationCount + SignalHistoryCount);

				for (int i = 0; i < slowEMAData.Rows.Count; ++i)
					chartData.Rows.Add(Convert.ToDouble(fastEMAData.Rows[i]["ema"]) - Convert.ToDouble(slowEMAData.Rows[i]["ema"]), 0);

				DataTable signaEMAData = Indicator.GenerateExponentialMovingAverage(chartData, "macd", SignalHistoryCount, calculationCount);

				for (int i = 0; i < SignalHistoryCount; ++i)
					chartData.Rows.RemoveAt(0);

				for (int i = 0; i < chartData.Rows.Count; ++i)
					chartData.Rows[i]["signal"] = signaEMAData.Rows[i]["ema"];

				return chartData;
			}
		}
	}
}