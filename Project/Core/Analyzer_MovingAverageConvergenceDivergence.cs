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

			private static int CalclationCount
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.CalclationCount; }
			}

			public static Result Analyze(Info Info)
			{
				DataTable data = Info.HistoryData;

				DataTable chartData = GenerateData(data);
				if (chartData == null)
					return null;

				if (ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.WriteToCSV)
				{
					DataTable tempData = data.DefaultView.ToTable();
					tempData.Columns.Add("macd");
					tempData.Columns.Add("signal");

					int startIndex = tempData.Rows.Count - chartData.Rows.Count;

					for (int i = 0; i < chartData.Rows.Count; ++i)
					{
						DataRow chartRow = chartData.Rows[i];
						DataRow tempDataRow = tempData.Rows[startIndex + i];

						tempDataRow["macd"] = chartRow["macd"];
						tempDataRow["signal"] = chartRow["signal"];
					}

					Analyzer.WriteCSV(ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.CSVPath, Info, tempData);
				}

				int lastIndex = chartData.Rows.Count - 1;

				double lastMACD = Convert.ToDouble(chartData.Rows[lastIndex]["macd"]);
				double lastSignal = Convert.ToDouble(chartData.Rows[lastIndex]["signal"]);

				double prevMACD = Convert.ToDouble(chartData.Rows[lastIndex - 1]["macd"]);
				double prevSignal = Convert.ToDouble(chartData.Rows[lastIndex - 1]["signal"]);

				int action = 0;
				double worthiness = 0;

				if (prevMACD <= prevSignal && lastMACD >= lastSignal)
				{
					action = 1;
					worthiness = 0.5F;
				}
				else if (prevMACD >= prevSignal && lastMACD <= lastSignal)
				{
					action = -1;
					worthiness = 0.5F;
				}
				else if (prevMACD <= 0 && 0 <= lastMACD)
				{
					action = 1;
					worthiness = 1;
				}
				else if (prevMACD >= 0 && 0 >= lastMACD)
				{
					action = -1;
					worthiness = 1;
				}

				return new Result() { Action = action, Worthiness = worthiness };
			}

			private static DataTable GenerateData(DataTable Data)
			{
				if (FastHistoryCount <= 0)
					return null;

				if (SlowHistoryCount <= 0 || SlowHistoryCount < FastHistoryCount)
					return null;

				if (CalclationCount < 3)
					return null;

				int calculationCount = Math.Min(Math.Max(1, Data.Rows.Count - SlowHistoryCount - SignalHistoryCount), CalclationCount);

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

			private static DataTable GenerateSignalData(DataTable Data)
			{
				if (SignalHistoryCount <= 0)
					return null;
				return null;
			}
		}
	}
}