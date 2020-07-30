using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public static partial class Analyzer
	{
		public static class SimpleMovingAverage
		{
			//https://commodity.com/technical-analysis/ma-simple/
			//https://www.dummies.com/personal-finance/investing/stocks-trading/how-to-calculate-simple-moving-average-in-trading/

			private static int SlowHistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.SimpleMovingAverage.SlowHistoryCount; }
			}

			private static int FastHistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.SimpleMovingAverage.FastHistoryCount; }
			}

			private static int CalculationCount
			{
				get { return ConfigManager.Config.DataAnalyzer.SimpleMovingAverage.CalculationCount; }
			}

			public static Result Analyze(Info Info)
			{
				if (!ConfigManager.Config.DataAnalyzer.SimpleMovingAverage.Enabled)
					return null;

				if (CalculationCount < ConfigManager.Config.DataAnalyzer.BacklogCount + 1)
				{
					ConsoleHelper.WriteError("CalculationCount must be grater than {0}, current value is {1}", ConfigManager.Config.DataAnalyzer.BacklogCount, CalculationCount);
					return null;
				}

				DataTable data = Info.HistoryData;

				int maxRowCount = 0;

				int calculationCount = Math.Min(Math.Max(ConfigManager.Config.DataAnalyzer.BacklogCount + 1, data.Rows.Count - (SlowHistoryCount - 1)), CalculationCount);
				DataTable slowSMAData = Analyzer.GenerateSimpleMovingAverageData(data, "close", SlowHistoryCount, calculationCount);

				calculationCount = Math.Min(Math.Max(ConfigManager.Config.DataAnalyzer.BacklogCount + 1, data.Rows.Count - (FastHistoryCount - 1)), CalculationCount);
				DataTable fastSMAData = Analyzer.GenerateSimpleMovingAverageData(data, "close", FastHistoryCount, calculationCount);

				DataTable tempChartData = new DataTable();
				tempChartData.Columns.Add("sma_" + SlowHistoryCount);
				tempChartData.Columns.Add("sma_" + FastHistoryCount);
				for (int i = 0; i < maxRowCount; ++i)
					tempChartData.Rows.Add();

				int startIndex = tempChartData.Rows.Count - slowSMAData.Rows.Count;
				for (int j = 0; j < slowSMAData.Rows.Count; ++j)
					tempChartData.Rows[startIndex + j][0] = slowSMAData.Rows[j]["sma"];

				startIndex = tempChartData.Rows.Count - fastSMAData.Rows.Count;
				for (int j = 0; j < fastSMAData.Rows.Count; ++j)
					tempChartData.Rows[startIndex + j][0] = fastSMAData.Rows[j]["sma"];

				Result result = new Result() { Signals = new Signal[ConfigManager.Config.DataAnalyzer.BacklogCount], Data = tempChartData };

				for (int i = 0; i < result.Signals.Length; ++i)
				{
					int index = fastSMAData.Rows.Count - 1 - i;
					double prevFastSMA = Convert.ToDouble(fastSMAData.Rows[index - 1]["sma"]);
					double currFastSMA = Convert.ToDouble(fastSMAData.Rows[index]["sma"]);

					index = slowSMAData.Rows.Count - 1 - i;
					double prevSlowSMA = Convert.ToDouble(slowSMAData.Rows[index - 1]["sma"]);
					double currSlowSMA = Convert.ToDouble(slowSMAData.Rows[index]["sma"]);

					int action = 0;
					double worthiness = 0;
					Analyzer.CheckCrossover(prevFastSMA, currFastSMA, prevSlowSMA, currSlowSMA, out action, out worthiness);

					result.Signals[result.Signals.Length - 1 - i] = new Signal() { Action = action, Worthiness = worthiness };
				}

				return result;
			}
		}
	}
}