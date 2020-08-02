using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public static partial class Indicator
	{
		public class Info
		{
			public DataTable HistoryData;
		}

		public static DataTable GenerateSimpleMovingAverageData(DataTable Data, string ColumnName, int BacklogCount, int CalculationCount)
		{
			if (BacklogCount < 1)
			{
				ConsoleHelper.WriteError("HistoryCount must be grater than 0, current value is {0}", BacklogCount);
				return null;
			}

			int requiredCount = (BacklogCount + CalculationCount) - 1;

			if (Data.Rows.Count < requiredCount)
				return null;

			int startIndex = Data.Rows.Count - requiredCount;

			DataTable smaData = new DataTable();
			//smaData.Columns.Add("date", typeof(DateTime));
			smaData.Columns.Add("sma", typeof(double));

			double tailSum = 0;
			for (int i = startIndex; i < Data.Rows.Count; ++i)
			{
				DataRow row = Data.Rows[i];

				tailSum += Convert.ToDouble(row[ColumnName]);

				if (i + 1 >= startIndex + BacklogCount)
				{
					//smaData.Rows.Add(row["take_time"], tailSum / BacklogCount);
					smaData.Rows.Add(tailSum / BacklogCount);

					tailSum -= Convert.ToDouble(Data.Rows[i - (BacklogCount - 1)][ColumnName]);
				}
			}

			return smaData;
		}

		public static DataTable GenerateExponentialMovingAverage(DataTable Data, string ColumnName, int BacklogCount, int CalculationCount)
		{
			if (BacklogCount < 1)
			{
				ConsoleHelper.WriteError("HistoryCount must be grater than 0, current value is {0}", BacklogCount);
				return null;
			}

			int requiredCount = BacklogCount + CalculationCount;

			if (Data.Rows.Count < requiredCount)
				return null;

			double k = 2 / (float)(BacklogCount + 1);

			int startIndex = Data.Rows.Count - requiredCount;

			double lastEMA = 0;
			for (int i = 0; i < BacklogCount; ++i)
				lastEMA += Convert.ToInt32(Data.Rows[startIndex + i][ColumnName]);
			lastEMA /= BacklogCount;

			startIndex = Data.Rows.Count - CalculationCount;

			DataTable emaData = new DataTable();
			//emaData.Columns.Add("date", typeof(DateTime));
			emaData.Columns.Add("ema", typeof(double));

			for (int i = startIndex; i < Data.Rows.Count; ++i)
			{
				DataRow row = Data.Rows[i];

				double ema = (Convert.ToInt32(row[ColumnName]) * k) + (lastEMA * (1 - k));

				//emaData.Rows.Add(row["take_time"], tailSum / BacklogCount);
				emaData.Rows.Add(ema);

				lastEMA = ema;
			}

			return emaData;
		}
	}
}