﻿using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Text;

namespace Core
{
	public class DataAnalyzer : Worker
	{
		private static readonly Func<Analyzer.Info, Analyzer.Result>[] Analyzers = new Func<Analyzer.Info, Analyzer.Result>[] { Analyzer.RelativeStrengthIndex.Analyze, Analyzer.MovingAverageConvergenceDivergence.Analyze };

		public override float WorkHour
		{
			get { return ConfigManager.Config.DataAnalyzer.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			DataTable stocksTable = Data.Database.QueryDataTable("SELECT id, symbol FROM stocks");

			string dateTime = CurrentDateTime.ToDatabaseDateTime();

			int totalProcessCount = stocksTable.Rows.Count * Analyzers.Length;
			int totalProcessedCount = 0;

			StringBuilder query = new StringBuilder();
			for (int i = 0; i < stocksTable.Rows.Count; ++i)
			{
				DataRow row = stocksTable.Rows[i];

				int id = Convert.ToInt32(row["id"]);

				DataTable historyTable = Data.Database.QueryDataTable("SELECT take_time, count, volume, value, open, first, high, low, last, close FROM snapshots WHERE stock_id=@stock_id AND DATE(take_time)<=DATE(@current_date) ORDER BY take_time",
					"stock_id", id,
					"current_date", CurrentDateTime);

				Analyzer.Info info = new Analyzer.Info { DateTime = CurrentDateTime, ID = id, Symbol = row["symbol"].ToString(), HistoryData = historyTable };

				double worthiness = 0;
				int availableResultCount = 0;
				for (int j = 0; j < Analyzers.Length; ++j)
				{
					var analyzer = Analyzers[j];

					Analyzer.Result result = analyzer(info);

					if (result != null)
					{
						worthiness += result.Action * result.Worthiness;

						++availableResultCount;
					}

					ConsoleHelper.WriteInfo("Analyzing data {0}%", (int)(++totalProcessedCount / (float)totalProcessCount * 100));
				}

				worthiness /= availableResultCount;

				if (worthiness != 0)
				{
					query.Append("INSERT INTO analyzes(stock_id, analyze_time, action, worthiness) VALUES(");
					query.Append(id);
					query.Append(",'");
					query.Append(dateTime);
					query.Append("',");
					query.Append(Math.Sign(worthiness));
					query.Append(',');
					query.Append(Math.Abs(worthiness));
					query.Append(");");
				}
			}

			if (query.Length != 0)
				Data.Database.Execute(query.ToString());

			return true;
		}
	}
}