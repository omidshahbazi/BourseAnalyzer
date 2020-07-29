﻿using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Text;

namespace Core
{
	public static partial class Analyzer
	{
		public class Info
		{
			public DateTime DateTime;

			public int ID;

			public string Symbol;

			public DataTable HistoryData;
		}

		public class Signal
		{
			public int Action;
			public double Worthiness;
		}

		public class Result
		{
			public Signal[] Signals;
			public DataTable Data;
		}

		private static DataTable GenerateSMAData(DataTable Data, string ColumnName, int BacklogCount, int CalculationCount)
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
			smaData.Columns.Add("date", typeof(DateTime));
			smaData.Columns.Add("sma", typeof(double));

			double tailSum = 0;
			for (int i = startIndex; i < Data.Rows.Count; ++i)
			{
				DataRow row = Data.Rows[i];

				tailSum += Convert.ToDouble(row[ColumnName]);

				if (i + 1 >= startIndex + BacklogCount)
				{
					smaData.Rows.Add(row["take_time"], tailSum / BacklogCount);

					tailSum -= Convert.ToDouble(Data.Rows[i - (BacklogCount - 1)][ColumnName]);
				}
			}

			return smaData;
		}
	}
}