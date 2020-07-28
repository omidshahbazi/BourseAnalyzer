using GameFramework.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Core
{
	public class DataAnalyzer : Worker
	{
		private static readonly Func<Analyzer.Info, Analyzer.Result[]>[] Analyzers = new Func<Analyzer.Info, Analyzer.Result[]>[] { Analyzer.RelativeStrengthIndex.Analyze, Analyzer.MovingAverageConvergenceDivergence.Analyze, Analyzer.SimpleMovingAverage.Analyze, Analyzer.AwesomeOscillator.Analyze };

		public override bool Enabled
		{
			get { return ConfigManager.Config.DataAnalyzer.Enabled; }
		}

		public override float WorkHour
		{
			get { return ConfigManager.Config.DataAnalyzer.WorkHour; }
		}

		public int BacklogCount
		{
			get { return ConfigManager.Config.DataAnalyzer.BacklogCount; }
		}

		public int MinimumTradeCount
		{
			get { return ConfigManager.Config.DataAnalyzer.MinimumTradeCount; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			if (BacklogCount < 1)
			{
				ConsoleHelper.WriteError("BacklogCount must be grater than 1, current value is {0}", BacklogCount);
				return false;
			}

			DataTable stocksTable = Data.QueryDataTable("SELECT id, symbol FROM stocks");

			string dateTime = CurrentDateTime.ToDatabaseDateTime();

			int totalProcessedCount = 0;
			int lastPercent = 0;

			Analyzer.Result[][] results = new Analyzer.Result[Analyzers.Length][];
			int lastResultIndex = BacklogCount - 1;

			StringBuilder query = new StringBuilder();
			for (int i = 0; i < stocksTable.Rows.Count; ++i)
			{
				DataRow row = stocksTable.Rows[i];

				int id = Convert.ToInt32(row["id"]);

				DataTable historyTable = Data.QueryDataTable("SELECT take_time, count, volume, value, open, first, high, low, last, close, ((high-low)/2) median FROM snapshots WHERE stock_id=@stock_id AND DATE(take_time)<=DATE(@current_date) ORDER BY take_time",
					"stock_id", id,
					"current_date", CurrentDateTime);

				if (historyTable.Rows.Count == 0 || Convert.ToInt32(historyTable.Rows[historyTable.Rows.Count - 1]["count"]) < MinimumTradeCount)
					continue;

				Analyzer.Info info = new Analyzer.Info { DateTime = CurrentDateTime, ID = id, Symbol = row["symbol"].ToString(), HistoryData = historyTable };

				double worthiness = 0;
				int availableResultCount = 0;
				for (int j = 0; j < Analyzers.Length; ++j)
				{
					var analyzer = Analyzers[j];

					results[j] = analyzer(info);
				}

				for (int j = 0; j < Analyzers.Length; ++j)
				{
					if (results[j] == null)
						continue;

					Analyzer.Result result = results[j][lastResultIndex];

					if (result.Action == 0)
						continue;

					worthiness += result.Action * result.Worthiness;
					++availableResultCount;

					for (int k = 0; k < Analyzers.Length; ++k)
					{
						if (j == k)
							continue;

						if (results[k] == null)
							continue;

						for (int l = lastResultIndex; l > -1; --l)
						{
							Analyzer.Result refResult = results[k][l];

							if (refResult.Action == 0)
								continue;

							worthiness += refResult.Action * refResult.Worthiness;
							++availableResultCount;

							break;
						}
					}
				}

				if (availableResultCount > 1)
				{
					worthiness /= availableResultCount;

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

				int percent = (int)(++totalProcessedCount / (float)stocksTable.Rows.Count * 100);
				if (lastPercent != percent)
				{
					ConsoleHelper.WriteInfo("Analyzing data {0}%", percent);
					lastPercent = percent;
				}
			}

			if (query.Length != 0)
				Data.Execute(query.ToString());

			return true;
		}
	}
}