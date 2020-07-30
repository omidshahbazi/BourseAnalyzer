using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public static partial class Analyzer
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

			private static float IgnoreThreshold
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.IgnoreThreshold; }
			}

			private static int PostPeriodCount
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.PostPeriodCount; }
			}

			public static Result Analyze(Info Info)
			{
				if (!ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.Enabled)
					return null;

				if (PostPeriodCount < 0)
				{
					ConsoleHelper.WriteError("PostPeriodCount cannot be negative, current value is {1}", PostPeriodCount);
					return null;
				}

				if (PostPeriodCount != 0 && IgnoreThreshold < 0)
				{
					ConsoleHelper.WriteError("IgnoreThreshold must be grater than 0, current value is {1}", IgnoreThreshold);
					return null;
				}

				DataTable data = Info.HistoryData;

				DataTable chartData = GenerateData(data);
				if (chartData == null)
					return null;

				Result result = new Result() { Signals = new Signal[ConfigManager.Config.DataAnalyzer.BacklogCount], Data = chartData };

				for (int i = 0; i < result.Signals.Length; ++i)
				{
					int action = 0;
					double worthiness = 0;

					int index = chartData.Rows.Count - 1 - i - PostPeriodCount;

					if (index > 0)
					{
						double prevMACD = Convert.ToDouble(chartData.Rows[index - 1]["macd"]);
						double currMACD = Convert.ToDouble(chartData.Rows[index]["macd"]);

						double prevSignal = Convert.ToDouble(chartData.Rows[index - 1]["signal"]);
						double currSignal = Convert.ToDouble(chartData.Rows[index]["signal"]);

						int close = Convert.ToInt32(data.Rows[data.Rows.Count - 1 - i]["close"]);
						double threshold = (close == 0 ? 0 : Math.Abs(currMACD - currSignal) / close);
						if (PostPeriodCount == 0 || threshold >= IgnoreThreshold)
						{
							if ((prevMACD <= prevSignal && currMACD > currSignal) ||
								(prevMACD < prevSignal && currMACD >= currSignal))
							{
								action = 1;
								worthiness = 1;
							}
							else if ((prevMACD >= prevSignal && currMACD < currSignal) ||
									 (prevMACD > prevSignal && currMACD <= currSignal))
							{
								action = -1;
								worthiness = 1;
							}
							else if ((prevMACD <= 0 && 0 < currMACD) ||
									 (prevMACD < 0 && 0 <= currMACD))
							{
								action = 1;
								worthiness = 0.5F;
							}
							else if ((prevMACD >= 0 && 0 > currMACD) ||
									  (prevMACD > 0 && 0 >= currMACD))
							{
								action = -1;
								worthiness = 0.5F;
							}
						}
					}

					result.Signals[result.Signals.Length - 1 - i] = new Signal() { Action = action, Worthiness = worthiness };
				}

				//if (ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.WriteToFile)
				//{
				//	DataTable tempData = data.DefaultView.ToTable();
				//	tempData.Columns.Add("macd");
				//	tempData.Columns.Add("signal");

				//	int startIndex = tempData.Rows.Count - chartData.Rows.Count;

				//	for (int i = 0; i < chartData.Rows.Count; ++i)
				//	{
				//		DataRow chartRow = chartData.Rows[i];
				//		DataRow tempDataRow = tempData.Rows[startIndex + i];

				//		tempDataRow["macd"] = chartRow["macd"];
				//		tempDataRow["signal"] = chartRow["signal"];
				//	}

				//	Analyzer.WriteCSV(ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.Path, Info, tempData);
				//}

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

				if (CalculationCount < ConfigManager.Config.DataAnalyzer.BacklogCount)
				{
					ConsoleHelper.WriteError("CalculationCount must be grater than {0}, current value is {1}", ConfigManager.Config.DataAnalyzer.BacklogCount, CalculationCount);
					return null;
				}

				int calculationCount = Math.Min(Math.Max(ConfigManager.Config.DataAnalyzer.BacklogCount + 1, Data.Rows.Count - SlowHistoryCount - SignalHistoryCount), CalculationCount);

				int requiredCount = calculationCount + SlowHistoryCount + SignalHistoryCount;

				if (Data.Rows.Count < requiredCount)
					return null;

				DataTable chartData = new DataTable();
				chartData.Columns.Add("slow_ema", typeof(double));
				chartData.Columns.Add("fast_ema", typeof(double));
				chartData.Columns.Add("macd", typeof(double));
				chartData.Columns.Add("signal", typeof(double));
				for (int i = 0; i < requiredCount; ++i)
					chartData.Rows.Add(0, 0, 0, 0);

				CalculateExponentialMovingAverage(Data, chartData, "close", "slow_ema", calculationCount + SignalHistoryCount, SlowHistoryCount);
				CalculateExponentialMovingAverage(Data, chartData, "close", "fast_ema", calculationCount + SignalHistoryCount, FastHistoryCount);

				for (int i = 0; i < chartData.Rows.Count; ++i)
				{
					DataRow row = chartData.Rows[i];

					row["macd"] = Convert.ToDouble(row["fast_ema"]) - Convert.ToDouble(row["slow_ema"]);
				}

				for (int i = 0; i < SlowHistoryCount; ++i)
					chartData.Rows.RemoveAt(0);

				CalculateExponentialMovingAverage(chartData, chartData, "macd", "signal", calculationCount, SignalHistoryCount);

				chartData.Columns.Remove("slow_ema");
				chartData.Columns.Remove("fast_ema");

				for (int i = 0; i < SignalHistoryCount; ++i)
					chartData.Rows.RemoveAt(0);

				return chartData;
			}

			private static void CalculateExponentialMovingAverage(DataTable Data, DataTable ChartData, string SourceColumnName, string ResultColumnName, int CalculationCount, int HistoryCount)
			{
				double k = 2 / (float)(HistoryCount + 1);

				int startIndex = Data.Rows.Count - CalculationCount - HistoryCount;

				double lastEMA = 0;
				for (int i = 0; i < HistoryCount; ++i)
					lastEMA += Convert.ToInt32(Data.Rows[startIndex + i][SourceColumnName]);
				lastEMA /= HistoryCount;

				startIndex = Data.Rows.Count - CalculationCount;
				int chartDataStartIndex = ChartData.Rows.Count - CalculationCount;

				for (int i = 0; i < CalculationCount; ++i)
				{
					double ema = (Convert.ToInt32(Data.Rows[startIndex + i][SourceColumnName]) * k) + (lastEMA * (1 - k));

					ChartData.Rows[chartDataStartIndex + i][ResultColumnName] = ema;

					lastEMA = ema;
				}
			}
		}
	}
}