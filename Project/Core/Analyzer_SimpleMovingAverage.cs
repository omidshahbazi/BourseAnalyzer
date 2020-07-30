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

			private static int[] HistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.SimpleMovingAverage.HistoryCount; }
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

				DataTable[] smaDataTables = new DataTable[HistoryCount.Length];
				int maxRowCount = 0;

				for (int i = 0; i < HistoryCount.Length; ++i)
				{
					int historyCount = HistoryCount[i];

					if (data.Rows.Count < historyCount + 1)
						return null;

					int calculationCount = Math.Min(Math.Max(ConfigManager.Config.DataAnalyzer.BacklogCount + 1, data.Rows.Count - (historyCount - 1)), CalculationCount);

					smaDataTables[i] = Analyzer.GenerateSMAData(data, "close", historyCount, calculationCount);

					if (smaDataTables[i] != null && maxRowCount < smaDataTables[i].Rows.Count)
						maxRowCount = smaDataTables[i].Rows.Count;
				}

				Result result = null;

				int shortTermHistoryIndex = Array.IndexOf(HistoryCount, MathHelper.Min(HistoryCount));
				DataTable shortTermData = smaDataTables[shortTermHistoryIndex];
				if (shortTermData != null)
				{
					int longTermHistoryIndex = Array.IndexOf(HistoryCount, MathHelper.Max(HistoryCount));
					DataTable longTermData = smaDataTables[longTermHistoryIndex];
					if (longTermData != null)
					{
						DataTable tempChartData = new DataTable();
						for (int i = 0; i < maxRowCount; ++i)
							tempChartData.Rows.Add();

						for (int i = 0; i < HistoryCount.Length; ++i)
						{
							string columnName = "sma_" + HistoryCount[i];

							tempChartData.Columns.Add(columnName);

							DataTable smaData = smaDataTables[i];
							if (smaData == null)
								continue;

							int startIndex = tempChartData.Rows.Count - smaData.Rows.Count;

							for (int j = 0; j < smaData.Rows.Count; ++j)
								tempChartData.Rows[startIndex + j][columnName] = smaData.Rows[j]["sma"];
						}

						result = new Result() { Signals = new Signal[ConfigManager.Config.DataAnalyzer.BacklogCount], Data = tempChartData };

						for (int i = 0; i < result.Signals.Length; ++i)
						{
							int index = shortTermData.Rows.Count - 1 - i;
							double prevShortSMA = Convert.ToDouble(shortTermData.Rows[index - 1]["sma"]);
							double currShortSMA = Convert.ToDouble(shortTermData.Rows[index]["sma"]);

							index = longTermData.Rows.Count - 1 - i;
							double prevLongSMA = Convert.ToDouble(longTermData.Rows[index - 1]["sma"]);
							double currLongSMA = Convert.ToDouble(longTermData.Rows[index]["sma"]);

							int action = 0;
							double worthiness = 0;
							Analyzer.CheckCrossover(prevShortSMA, currShortSMA, prevLongSMA, currLongSMA, out action, out worthiness);

							//if ((prevShortSMA <= prevLongSMA && currShortSMA > currLongSMA) ||
							//	(prevShortSMA < prevLongSMA && currShortSMA >= currLongSMA))
							//{
							//	action = 1;
							//	worthiness = 1;
							//}
							//else if ((prevShortSMA >= prevLongSMA && currShortSMA < currLongSMA) ||
							//		 (prevShortSMA > prevLongSMA && currShortSMA <= currLongSMA))
							//{
							//	action = -1;
							//	worthiness = 1;
							//}

							result.Signals[result.Signals.Length - 1 - i] = new Signal() { Action = action, Worthiness = worthiness };
						}
					}
				}

				return result;
			}
		}
	}
}