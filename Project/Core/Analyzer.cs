using System;
using System.Collections.Generic;
using System.Data;

namespace Core
{
	public static class Analyzer
	{
		private class DataRowList : List<DataRow>
		{ }

		public class Info
		{
			public int ID
			{
				get;
				private set;
			}

			public string Symbol
			{
				get;
				private set;
			}

			public DataTable HistoryData
			{
				get;
				private set;
			}

			public DataTable LiveData
			{
				get;
				private set;
			}

			public Info(int ID, string Symbol, DataTable HistoryData, DataTable LiveData)
			{
				this.ID = ID;
				this.Symbol = Symbol;
				this.HistoryData = HistoryData;
				this.LiveData = LiveData;
			}
		}

		public class TendLine
		{
			public static void Analyze(Info Info)
			{
				DataRowList highestHighs = new DataRowList();
				for (int i = 1; i < Info.HistoryData.Rows.Count - 1; ++i)
				{
					DataRow prevRow = Info.HistoryData.Rows[i - 1];
					DataRow currRow = Info.HistoryData.Rows[i];
					DataRow nextRow = Info.HistoryData.Rows[i + 1];

					int prevClose = Convert.ToInt32(prevRow["close"]);
					int currClose = Convert.ToInt32(currRow["close"]);
					int nextClose = Convert.ToInt32(nextRow["close"]);

					if (prevClose > currClose || nextClose > currClose)
						continue;

					highestHighs.Add(currRow);
				}
			}
		}

		public static class DirectionChange
		{
			public static void Analyze(Info Info)
			{
				List<int> closes = new List<int>();

				int prevSign = 0;
				for (int i = 0; i < Info.HistoryData.Rows.Count; ++i)
				{
					DataRow row = Info.HistoryData.Rows[i];

					int close = Convert.ToInt32(row["close"]);

					int prevClose = 0;
					Info.LiveData.DefaultView.RowFilter = "symbol='" + Info.Symbol + "'";
					if (Info.LiveData.DefaultView.Count != 0)
						prevClose = Convert.ToInt32(Info.LiveData.DefaultView[0]["close"]);

					int sign = Math.Sign(close - prevClose);

					if (sign == 0)
						continue;

					float rate = close / (float)prevClose;

					rate *= 100;

					if (prevSign != sign)
					{
						Data.Database.Execute("INSERT INTO analyze_results(stock_id, analyze_time, action, action_time) VALUES(@stock_id, NOW(), @action, NOW())", //TIMESTAMPADD(second, 6 * 3600, TIMESTAMPADD(DAY, 1, DATE(NOW())))
							"stock_id", Info.ID,
							"action", sign);

						closes.Clear();
						closes.Add(prevClose);
					}

					closes.Add(close);

					prevSign = sign;
				}
			}
		}
	}
}