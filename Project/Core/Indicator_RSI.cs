using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public static partial class Indicator
	{
		public static class RSI
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

			private static float MaxRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.MaxRSI; }
			}

			public static DataTable Generate(Info Info)
			{
				DataTable data = Info.HistoryData;

				if (HistoryCount <= 0)
				{
					ConsoleHelper.WriteError("HistoryCount must be grater than 0, current value is {0}", HistoryCount);
					return null;
				}

				if (CalculationCount < ConfigManager.Config.DataAnalyzer.BacklogCount + 1)
				{
					ConsoleHelper.WriteError("CalculationCount must be grater than {0}, current value is {1}", ConfigManager.Config.DataAnalyzer.BacklogCount, CalculationCount);
					return null;
				}

				int calculationCount = Math.Min(Math.Max(ConfigManager.Config.DataAnalyzer.BacklogCount + 1, data.Rows.Count - HistoryCount + 1), CalculationCount);

				int requiredCount = HistoryCount - 1 + calculationCount;

				if (data.Rows.Count < requiredCount)
					return null;

				int startIndex = data.Rows.Count - requiredCount;

				double gainAvg = 0;
				double lossAvg = 0;

				DataTable rsiData = new DataTable();
				rsiData.Columns.Add("gain", typeof(int));
				rsiData.Columns.Add("loss", typeof(int));
				rsiData.Columns.Add("rsi", typeof(double));

				for (int i = 0; i < requiredCount; ++i)
				{
					DataRow row = data.Rows[startIndex + i];

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