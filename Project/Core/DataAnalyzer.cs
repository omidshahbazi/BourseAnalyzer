using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Text;

namespace Core
{
	public class DataAnalyzer : Worker
	{
		private static readonly Func<Analyzer.Info, Analyzer.Result>[] Analyzers = new Func<Analyzer.Info, Analyzer.Result>[] { Analyzer.RelativeStrengthIndex.Analyze };

		public override float WorkHour
		{
			get { return ConfigManager.Config.DataAnalyzer.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			ConsoleHelper.WriteInfo("Downloading live stocks info...");

			DataTable liveTable = DataDownloader.DownloadLiveData();
			if (liveTable == null)
				return false;

			DataTable stocksTable = Data.Database.QueryDataTable("SELECT id, symbol FROM stocks");

			DataTable analyzesTable = Data.Database.QueryDataTable("SELECT stock_id, first_snapshot_id FROM analyzes WHERE DATE(analyze_time)<=DATE(@current_date) ORDER BY analyze_time LIMIT @count",
				"count", stocksTable.Rows.Count,
				"current_date", CurrentDateTime);

			string dateTime = CurrentDateTime.ToDatabaseDateTime();

			StringBuilder query = new StringBuilder();

			for (int i = 0; i < stocksTable.Rows.Count; ++i)
			{
				DataRow row = stocksTable.Rows[i];

				int id = Convert.ToInt32(row["id"]);

				DataTable historyTable = Data.Database.QueryDataTable("SELECT id, take_time, count, volume, value, open, first, high, low, last, close, UNIX_TIMESTAMP(take_time) - UNIX_TIMESTAMP('2015/01/01') relative_time FROM snapshots WHERE stock_id=@stock_id AND DATE(take_time)<=DATE(@current_date) ORDER BY take_time",
					"stock_id", id,
					"current_date", CurrentDateTime);

				Analyzer.Info info = new Analyzer.Info { ID = id, Symbol = row["symbol"].ToString(), HistoryData = historyTable, LiveData = liveTable, AnalyzesData = analyzesTable };

				Analyzer.Result finalResult = null;
				for (int j = 0; j < Analyzers.Length; ++j)
				{
					var analyzer = Analyzers[j];

					ConsoleHelper.WriteInfo("Analyzing {0}...", analyzer.Method.DeclaringType.Name);

					Analyzer.Result result = analyzer(info);

					if (result == null)
						continue;

					finalResult = result;
				}

				if (finalResult != null && finalResult.Action != 0)
				{
					query.Append("INSERT INTO analyzes(stock_id, analyze_time, action, worthiness, first_snapshot_id) VALUES(");
					query.Append(id);
					query.Append(",'");
					query.Append(dateTime);
					query.Append("',");
					query.Append(finalResult.Action);
					query.Append(',');
					query.Append(finalResult.Worthiness);
					query.Append(',');
					query.Append(finalResult.FirstSnapshotID);
					query.Append(");");
				}
			}

			if (query.Length != 0)
				Data.Database.Execute(query.ToString());

			return true;
		}
	}
}