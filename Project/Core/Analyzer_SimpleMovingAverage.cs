using GameFramework.Common.Utilities;
using System;
using System.Collections.Generic;
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

				DataTable data = Info.HistoryData;

				DataTable[] smaDataTables = new DataTable[HistoryCount.Length];

				for (int i = 0; i < HistoryCount.Length; ++i)
					smaDataTables[i] = GenerateSMAData(data, HistoryCount[i]);

				int action = 0;
				double worthiness = 0;

				int shortTermHistoryIndex = Array.IndexOf(HistoryCount, MathHelper.Min(HistoryCount));
				DataTable shortTermData = smaDataTables[shortTermHistoryIndex];
				if (shortTermData != null)
				{
					int lastIndex = shortTermData.Rows.Count - 1;
					double lastShortSMA = Convert.ToDouble(shortTermData.Rows[lastIndex]["sma"]);
					double prevShortSMA = Convert.ToDouble(shortTermData.Rows[lastIndex - 1]["sma"]);

					int longTermHistoryIndex = Array.IndexOf(HistoryCount, MathHelper.Max(HistoryCount));
					DataTable longTermData = smaDataTables[longTermHistoryIndex];
					if (longTermData != null)
					{
						lastIndex = longTermData.Rows.Count - 1;
						double lastLongSMA = Convert.ToDouble(longTermData.Rows[lastIndex]["sma"]);
						double prevLongSMA = Convert.ToDouble(longTermData.Rows[lastIndex - 1]["sma"]);

						if ((prevShortSMA <= prevLongSMA && lastShortSMA > lastLongSMA) ||
							(prevShortSMA < prevLongSMA && lastShortSMA >= lastLongSMA))
						{
							action = 1;
							worthiness = 1;
						}
						else if ((prevShortSMA >= prevLongSMA && lastShortSMA < lastLongSMA) ||
								 (prevShortSMA > prevLongSMA && lastShortSMA <= lastLongSMA))
						{
							action = -1;
							worthiness = 1;
						}
					}
				}

				if (ConfigManager.Config.DataAnalyzer.SimpleMovingAverage.WriteToCSV)
				{
					DataTable tempData = data.DefaultView.ToTable();

					for (int i = 0; i < HistoryCount.Length; ++i)
					{
						string columnName = "sma_" + HistoryCount[i];

						tempData.Columns.Add(columnName);

						DataTable smaData = smaDataTables[i];
						if (smaData == null)
							continue;

						int startIndex = tempData.Rows.Count - smaData.Rows.Count;

						for (int j = 0; j < smaData.Rows.Count; ++j)
							tempData.Rows[startIndex + j][columnName] = smaData.Rows[j]["sma"];
					}

					Analyzer.WriteCSV(ConfigManager.Config.DataAnalyzer.SimpleMovingAverage.CSVPath, Info, action, tempData);
				}

				return new Result() { Action = action, Worthiness = worthiness };
			}

			private static DataTable GenerateSMAData(DataTable Data, int BacklogCount)
			{
				if (BacklogCount < 1)
				{
					ConsoleHelper.WriteError("HistoryCount must be grater than 0, current value is {0}", BacklogCount);
					return null;
				}

				if (CalculationCount < 1)
				{
					ConsoleHelper.WriteError("CalculationCount must be grater than 0, current value is {0}", CalculationCount);
					return null;
				}

				if (Data.Rows.Count < BacklogCount + 1)
					return null;

				int calculationCount = Math.Min(Data.Rows.Count - (BacklogCount - 1), CalculationCount);

				int requiredCount = (BacklogCount + calculationCount) - 1;

				int startIndex = Data.Rows.Count - requiredCount;

				DataTable smaData = new DataTable();
				smaData.Columns.Add("sma", typeof(double));

				int tailSum = 0;
				for (int i = startIndex; i < Data.Rows.Count; ++i)
				{
					DataRow row = Data.Rows[i];

					tailSum += Convert.ToInt32(row["close"]);

					if (i + 1 >= startIndex + BacklogCount)
					{
						smaData.Rows.Add(tailSum / (double)BacklogCount);

						tailSum -= Convert.ToInt32(Data.Rows[i - (BacklogCount - 1)]["close"]);
					}
				}

				return smaData;
			}
		}
	}
}