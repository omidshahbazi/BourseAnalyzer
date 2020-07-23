﻿using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.IO;
using System.Text;

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

			private static int CalclationCount
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.CalclationCount; }
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
				DataTable data = Info.HistoryData;

				if (LowRSI <= 0 || MidRSI <= LowRSI)
					return null;

				if (MidRSI <= LowRSI || HighRSI <= MidRSI)
					return null;

				if (HighRSI <= MidRSI || MaxRSI <= HighRSI)
					return null;

				DataTable rsiTable = GenerateRSIData(data);
				if (rsiTable == null)
					return null;

				if (ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.WriteToCSV)
				{
					DataTable tempData = data.DefaultView.ToTable();
					tempData.Columns.Add("rsi");

					int startIndex = tempData.Rows.Count - rsiTable.Rows.Count;

					for (int i = 0; i < rsiTable.Rows.Count; ++i)
					{
						DataRow row = tempData.Rows[startIndex + i];

						row["rsi"] = rsiTable.Rows[i]["rsi"];
					}

					Analyzer.WriteCSV(ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.CSVPath, Info, tempData);
				}

				int lastIndex = rsiTable.Rows.Count - 1;
				double prevRSI = Convert.ToDouble(rsiTable.Rows[lastIndex - 1]["rsi"]);
				double currRSI = Convert.ToDouble(rsiTable.Rows[lastIndex]["rsi"]);

				int action = 0;
				double worthiness = 0;

				if (prevRSI <= LowRSI && LowRSI < currRSI)
				{
					action = 1;
					worthiness = (LowRSI - prevRSI) / LowRSI;
				}
				else if (prevRSI <= MidRSI && MidRSI < currRSI)
				{
					action = 1;
					worthiness = (currRSI - prevRSI) / MaxRSI;
				}
				else if (HighRSI <= prevRSI && currRSI < HighRSI)
				{
					action = -1;
					worthiness = (prevRSI - HighRSI) / (MaxRSI - HighRSI);
				}
				else if (MidRSI <= prevRSI && currRSI < MidRSI)
				{
					action = -1;
					worthiness = (prevRSI - currRSI) / MaxRSI;
				}

				//if (action == 1)
				//	Console.WriteLine("Buy: {0} RSI: {1}% Worthiness: {2}%", Info.ID, (int)(currRSI * 100), (int)(worthiness * 100));
				//else if (action == -1)
				//	Console.WriteLine("Sell: {0} RSI: {1}% Worthiness: {2}%", Info.ID, (int)(currRSI * 100), (int)(worthiness * 100));

				return new Result() { Action = action, Worthiness = worthiness };
			}

			private static DataTable GenerateRSIData(DataTable Data)
			{
				if (HistoryCount <= 0)
					return null;

				if (CalclationCount < 2)
					return null;

				if (Data.Rows.Count < HistoryCount)
					return null;

				int calculationCount = Math.Min(Data.Rows.Count - HistoryCount + 1, CalclationCount);

				int requiredCount = HistoryCount + calculationCount - 1;

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