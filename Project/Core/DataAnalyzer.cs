using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public class DataAnalyzer : Worker
	{
		private static readonly Action<Analyzer.Info>[] Analyzers = new Action<Analyzer.Info>[] { Analyzer.RelativeStrengthIndex.Analyze };

		protected override float WorkHour
		{
			get { return 20.75F; }
		}

		protected override bool Do()
		{
			ConsoleHelper.WriteInfo("Analyzing data...");

			//DataTable liveTable = DataDownloader.Download();
			//if (liveTable == null)
			//	return false;

			DataTable liveTable = null;

			DataTable stocksTable = Data.Database.QueryDataTable("SELECT id, symbol FROM stocks");
			DataTable analyzesTable = Data.Database.QueryDataTable("SELECT stock_id, first_snapshot_id FROM analyzes ORDER BY analyze_time LIMIT @count", "count", stocksTable.Rows.Count);

			for (int i = 0; i < stocksTable.Rows.Count; ++i)
			{
				DataRow row = stocksTable.Rows[i];

				int id = Convert.ToInt32(row["id"]);

				DataTable historyTable = Data.Database.QueryDataTable("SELECT id, take_time, count, volume, value, open, first, high, low, last, close, UNIX_TIMESTAMP(take_time) - UNIX_TIMESTAMP('2015/01/01') relative_time FROM snapshots WHERE stock_id=@stock_id", "stock_id", id);

				Analyzer.Info info = new Analyzer.Info(id, row["symbol"].ToString(), historyTable, liveTable, analyzesTable);

				for (int j = 0; j < Analyzers.Length; ++j)
					Analyzers[j](info);
			}

			ConsoleHelper.WriteInfo("Analyzing data done");

			return true;
		}
	}
}