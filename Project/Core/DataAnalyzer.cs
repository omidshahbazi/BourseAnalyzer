using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public class DataAnalyzer : Worker
	{
		private static readonly Func<Analyzer.Info, Analyzer.Result>[] Analyzers = new Func<Analyzer.Info, Analyzer.Result>[] { Analyzer.RelativeStrengthIndex.Analyze };

		protected override float WorkHour
		{
			get { return ConfigManager.Config.Analyzer.WorkHour; }
		}

		protected override bool Do()
		{
			ConsoleHelper.WriteInfo("Analyzing data...");

			DataTable liveTable = DataDownloader.Download();
			if (liveTable == null)
				return false;

			DataTable stocksTable = Data.Database.QueryDataTable("SELECT id, symbol FROM stocks");
			DataTable analyzesTable = Data.Database.QueryDataTable("SELECT stock_id, first_snapshot_id FROM analyzes ORDER BY analyze_time LIMIT @count", "count", stocksTable.Rows.Count);

			for (int i = 0; i < stocksTable.Rows.Count; ++i)
			{
				DataRow row = stocksTable.Rows[i];

				int id = Convert.ToInt32(row["id"]);

				DataTable historyTable = Data.Database.QueryDataTable("SELECT id, take_time, count, volume, value, open, first, high, low, last, close, UNIX_TIMESTAMP(take_time) - UNIX_TIMESTAMP('2015/01/01') relative_time FROM snapshots WHERE stock_id=@stock_id", "stock_id", id);

				Analyzer.Info info = new Analyzer.Info { ID = id, Symbol = row["symbol"].ToString(), HistoryData = historyTable, LiveData = liveTable, AnalyzesData = analyzesTable };

				//for (int j = 0; j < Analyzers.Length; ++j)
				//{
				//	Analyzer.Result result = Analyzers[j](info);

				//	if (result == null)
				//		continue;

				//	?????
				//}

				Analyzer.Result result = Analyzers[0](info);
				if (result == null)
					continue;

				if (result.Action != 0)
					Data.Database.Execute("INSERT INTO analyzes(stock_id, analyze_time, action, worthiness, first_snapshot_id) VALUES(@stock_id, NOW(), @action, @worthiness, @first_snapshot_id)",
					"stock_id", id,
					"action", result.Action,
					"worthiness", result.Worthiness,
					"first_snapshot_id", result.FirstSnapshotID);
			}

			ConsoleHelper.WriteInfo("Analyzing data done");

			return true;
		}
	}
}