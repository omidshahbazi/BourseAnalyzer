using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Core
{
	public class DataAnalyzer : Worker
	{
		public override bool Enabled
		{
			get { return ConfigManager.Config.DataAnalyzer.Enabled; }
		}

		public override float WorkHour
		{
			get { return ConfigManager.Config.DataAnalyzer.WorkHour; }
		}

		public static int MinimumTradeCount
		{
			get { return ConfigManager.Config.DataAnalyzer.MinimumTradeCount; }
		}

		public static int BacklogCount
		{
			get { return ConfigManager.Config.DataAnalyzer.BacklogCount; }
		}

		public static int SignalConfirmationCount
		{
			get { return ConfigManager.Config.DataAnalyzer.SignalConfirmationCount; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			if (BacklogCount < 1)
			{
				ConsoleHelper.WriteError("BacklogCount must be grater than 1, current value is {0}", BacklogCount);
				return false;
			}

			if (SignalConfirmationCount < 0)
			{
				ConsoleHelper.WriteError("SignalConfirmationCount must be grater than 0, current value is {0}", SignalConfirmationCount);
				return false;
			}

			DataTable stocksTable = Data.QueryDataTable("SELECT id, symbol FROM stocks");

			string dateTime = CurrentDateTime.ToDatabaseDateTime();

			int totalProcessedCount = 0;
			int lastPercent = 0;

			StringBuilder query = new StringBuilder();
			for (int i = 0; i < stocksTable.Rows.Count; ++i)
			{
				int percent = (int)(++totalProcessedCount / (float)stocksTable.Rows.Count * 100);
				if (lastPercent != percent)
				{
					ConsoleHelper.WriteInfo("Analyzing data {0}%", percent);
					lastPercent = percent;
				}

				DataRow row = stocksTable.Rows[i];

				int id = Convert.ToInt32(row["id"]);
				string symbol = row["symbol"].ToString();
				id = 855;
				DataTable historyTable = Data.QueryDataTable("SELECT take_time, count, volume, value, open, first, high, low, last, close, ((high-low)/2) median FROM snapshots WHERE stock_id=@stock_id AND DATE(take_time)<=DATE(@current_date) ORDER BY take_time",
					"stock_id", id,
					"current_date", CurrentDateTime);

				if (historyTable.Rows.Count == 0)
					continue;

				DataRow lastRow = historyTable.Rows[historyTable.Rows.Count - 1];

				if (Convert.ToInt32(lastRow["count"]) < MinimumTradeCount || Convert.ToDateTime(lastRow["take_time"]).Date != CurrentDateTime.Date)
					continue;

				Indicator.Info info = new Indicator.Info { HistoryData = historyTable };

				Analyzer.AnalyzeInfo result = Analyzer.Analyze(info, BacklogCount);

				if (result.Worthiness != 0)
				{
					query.Append("INSERT INTO analyzes(stock_id, analyze_time, action, worthiness) VALUES(");
					query.Append(id);
					query.Append(",'");
					query.Append(dateTime);
					query.Append("',");
					query.Append(Math.Sign(result.Worthiness));
					query.Append(',');
					query.Append(Math.Abs(result.Worthiness));
					query.Append(");");
				}

				if (ConfigManager.Config.DataAnalyzer.WriteToFile)
				{
					StringBuilder builder = new StringBuilder();
					CSVWriter.Write(builder, 0, 0, historyTable);

					Helper.WriteToFile(ConfigManager.Config.DataAnalyzer.Path, CurrentDateTime, id + "_" + symbol + ".csv", builder.ToString());
				}
			}

			Data.Execute("DELETE FROM analyzes WHERE analyze_time=@time", "time", dateTime);

			if (query.Length != 0)
				Data.Execute(query.ToString());

			return true;
		}
	}
}