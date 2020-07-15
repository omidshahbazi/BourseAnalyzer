using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public class DataAnalyzer : Worker
	{
		//private static readonly Action<Analyzer.Info>[] Analyzers = new Action<Analyzer.Info>[] { Analyzer.TendLine.Analyze, Analyzer.DirectionChange.Analyze };
		private static readonly Action<Analyzer.Info>[] Analyzers = new Action<Analyzer.Info>[] { Analyzer.TendLine.Analyze };

		protected override float WorkHour
		{
			get { return 10.75F; }
		}

		protected override bool Do()
		{
			ConsoleHelper.WriteInfo("Analyzing data...");

			DataTable stocksTable = Data.Database.QueryDataTable("SELECT id, symbol FROM stocks");

			DataTable liveTable = DataDownloader.Download();
			if (liveTable == null)
				return false;

			for (int i = 0; i < stocksTable.Rows.Count; ++i)
			{
				DataRow row = stocksTable.Rows[i];

				int id = Convert.ToInt32(row["id"]);

				DataTable historyTable = Data.Database.QueryDataTable("SELECT take_time, count, volume, value, open, first, high, low, last, close FROM snapshots WHERE stock_id=@stock_id", "stock_id", id);

				Analyzer.Info info = new Analyzer.Info(id, row["symbol"].ToString(), historyTable, liveTable);

				for (int j = 0; j < Analyzers.Length; ++j)
					Analyzers[j](info);
			}

			ConsoleHelper.WriteInfo("Analyzing data done");

			return true;
		}
	}
}