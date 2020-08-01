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

				for (int i = 0; i < PostPeriodCount; ++i)
				{
					int index = chartData.Rows.Count - 1 - i;

					if (index < 1)
						break;

					double prevMACD = Convert.ToDouble(chartData.Rows[index - 1]["macd"]);
					double currMACD = Convert.ToDouble(chartData.Rows[index]["macd"]);

					double prevSignal = Convert.ToDouble(chartData.Rows[index - 1]["signal"]);
					double currSignal = Convert.ToDouble(chartData.Rows[index]["signal"]);

					int action = 0;
					if (Analyzer.CheckCrossover(prevMACD, currMACD, prevSignal, currSignal, out action))
						return null;
				}

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
							//if (!Analyzer.CheckCrossover(prevMACD, currMACD, prevSignal, currSignal, out action))
							//	Analyzer.CheckPointCrossover(prevMACD, currMACD, 0, out action);

							Analyzer.CheckCrossover(prevMACD, currMACD, prevSignal, currSignal, out action);

							if (action != 0)
							{
								DataTable smaData = Analyzer.GenerateSimpleMovingAverageData(data, "close", 9, 2);

								worthiness = Math.Abs((Convert.ToDouble(smaData.Rows[1]["sma"]) / Convert.ToDouble(smaData.Rows[0]["sma"])) - 1);
							}
						}
					}

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
				chartData.Columns.Add("macd", typeof(double));
				chartData.Columns.Add("signal", typeof(double));

				DataTable slowEMAData = Analyzer.GenerateExponentialMovingAverage(Data, "close", SlowHistoryCount, calculationCount + SignalHistoryCount);
				DataTable fastEMAData = Analyzer.GenerateExponentialMovingAverage(Data, "close", FastHistoryCount, calculationCount + SignalHistoryCount);

				for (int i = 0; i < slowEMAData.Rows.Count; ++i)
					chartData.Rows.Add(Convert.ToDouble(fastEMAData.Rows[i]["ema"]) - Convert.ToDouble(slowEMAData.Rows[i]["ema"]), 0);

				DataTable signaEMAData = Analyzer.GenerateExponentialMovingAverage(chartData, "macd", SignalHistoryCount, calculationCount);

				for (int i = 0; i < SignalHistoryCount; ++i)
					chartData.Rows.RemoveAt(0);

				for (int i = 0; i < chartData.Rows.Count; ++i)
					chartData.Rows[i]["signal"] = signaEMAData.Rows[i]["ema"];

				return chartData;
			}
		}
	}
}