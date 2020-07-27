﻿using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public static partial class Analyzer
	{
		public static class RelativeStrengthIndex
		{
			//https://blog.quantinsti.com/rsi-indicator/

			private static int HistoryCount
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.HistoryCount; }
			}

			private static int CalculationCount
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.CalculationCount; }
			}

			private static float LowRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.LowRSI; }
			}

			private static float MidRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.MidRSI; }
			}

			private static float HighRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.HighRSI; }
			}

			private static float MaxRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.MaxRSI; }
			}

			public static Result Analyze(Info Info)
			{
				if (!ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.Enabled)
					return null;

				DataTable data = Info.HistoryData;

				if (LowRSI <= 0 || MidRSI <= LowRSI)
				{
					ConsoleHelper.WriteError("LowRSI must be grater than 0 and smaller than MidRSI, current value is {0}", LowRSI);
					return null;
				}

				if (MidRSI <= LowRSI || HighRSI <= MidRSI)
				{
					ConsoleHelper.WriteError("MidRSI must be grater than LowRSI and smaller than HighRSI, current value is {0}", MidRSI);
					return null;
				}

				if (HighRSI <= MidRSI || MaxRSI <= HighRSI)
				{
					ConsoleHelper.WriteError("HighRSI must be grater than MidRSI and smaller than MaxRSI, current value is {0}", HighRSI);
					return null;
				}

				DataTable rsiData = GenerateRSIData(data);
				if (rsiData == null)
					return null;

				int lastIndex = rsiData.Rows.Count - 1;
				double prevRSI = Convert.ToDouble(rsiData.Rows[lastIndex - 1]["rsi"]);
				double currRSI = Convert.ToDouble(rsiData.Rows[lastIndex]["rsi"]);

				int action = 0;
				double worthiness = 0;

				if (prevRSI <= LowRSI && LowRSI < currRSI)
				{
					action = 1;
					worthiness = (LowRSI - prevRSI) / LowRSI;
				}
				if (HighRSI <= prevRSI && currRSI < HighRSI)
				{
					action = -1;
					worthiness = (prevRSI - HighRSI) / (MaxRSI - HighRSI);
				}
				else
				{
					lastIndex = data.Rows.Count - 1;
					double prevClose = Convert.ToDouble(data.Rows[lastIndex - 1]["close"]);
					double currClose = Convert.ToDouble(data.Rows[lastIndex]["close"]);

					if (prevRSI <= MidRSI && MidRSI < currRSI && prevClose < currClose)
					{
						action = 1;
						worthiness = (currRSI - prevRSI) / MaxRSI;
					}
					else if (MidRSI <= prevRSI && currRSI < MidRSI && prevClose > currClose)
					{
						action = -1;
						worthiness = (prevRSI - currRSI) / MaxRSI;
					}
				}

				if (ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.WriteToCSV)
				{
					DataTable tempData = data.DefaultView.ToTable();
					tempData.Columns.Add("rsi");

					int startIndex = tempData.Rows.Count - rsiData.Rows.Count;

					for (int i = 0; i < rsiData.Rows.Count; ++i)
					{
						DataRow row = tempData.Rows[startIndex + i];

						row["rsi"] = rsiData.Rows[i]["rsi"];
					}

					Analyzer.WriteCSV(ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.CSVPath, Info, action, tempData);
				}

				return new Result() { Action = action, Worthiness = worthiness };
			}

			private static DataTable GenerateRSIData(DataTable Data)
			{
				if (HistoryCount <= 0)
				{
					ConsoleHelper.WriteError("HistoryCount must be grater than 0, current value is {0}", HistoryCount);
					return null;
				}

				if (CalculationCount < 2)
				{
					ConsoleHelper.WriteError("CalculationCount must be grater than 1, current value is {0}", CalculationCount);
					return null;
				}

				if (Data.Rows.Count < HistoryCount + 1)
					return null;

				int calculationCount = Math.Min(Data.Rows.Count - HistoryCount + 1, CalculationCount);

				int requiredCount = HistoryCount - 1 + calculationCount;

				int startIndex = Data.Rows.Count - requiredCount;

				double gainAvg = 0;
				double lossAvg = 0;

				DataTable rsiData = new DataTable();
				rsiData.Columns.Add("gain", typeof(int));
				rsiData.Columns.Add("loss", typeof(int));
				rsiData.Columns.Add("rsi", typeof(double));

				for (int i = 0; i < requiredCount; ++i)
				{
					DataRow row = Data.Rows[startIndex + i];

					int open = Convert.ToInt32(row["open"]);
					int close = Convert.ToInt32(row["close"]);

					int gain = (open < close ? close - open : 0);
					int loss = (close < open ? open - close : 0);

					rsiData.Rows.Add(gain, loss, 0);

					if (i < HistoryCount)
					{
						gainAvg += gain;
						lossAvg += loss;
					}
				}

				gainAvg /= HistoryCount;
				lossAvg /= HistoryCount;

				startIndex = rsiData.Rows.Count - calculationCount;

				rsiData.Rows[startIndex++]["rsi"] = CalculateRSI(gainAvg, lossAvg);

				for (int i = 0; i < calculationCount - 1; ++i)
				{
					DataRow row = rsiData.Rows[startIndex + i];

					gainAvg = (gainAvg * (HistoryCount - 1) + Convert.ToInt32(row["gain"])) / HistoryCount;
					lossAvg = (lossAvg * (HistoryCount - 1) + Convert.ToInt32(row["loss"])) / HistoryCount;

					row["rsi"] = CalculateRSI(gainAvg, lossAvg);
				}

				rsiData.Columns.Remove("gain");
				rsiData.Columns.Remove("loss");

				for (int i = 0; i < HistoryCount - 1; ++i)
					rsiData.Rows.RemoveAt(0);

				return rsiData;
			}

			private static double CalculateRSI(double GainAverage, double LossAverage)
			{
				if (LossAverage == 0)
					return 1;

				return MaxRSI - (MaxRSI / (1 + (GainAverage / LossAverage)));
			}
		}
	}
}