using GameFramework.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Core
{
	public class DataAnalyzer : Worker
	{
		private static readonly Func<Analyzer.Info, Analyzer.Result>[] Analyzers = new Func<Analyzer.Info, Analyzer.Result>[] { Analyzer.RelativeStrengthIndex.Analyze, Analyzer.MovingAverageConvergenceDivergence.Analyze, Analyzer.SimpleMovingAverage.Analyze, Analyzer.AwesomeOscillator.Analyze };

		public override bool Enabled
		{
			get { return ConfigManager.Config.DataAnalyzer.Enabled; }
		}

		public override float WorkHour
		{
			get { return ConfigManager.Config.DataAnalyzer.WorkHour; }
		}

		public int MinimumTradeCount
		{
			get { return ConfigManager.Config.DataAnalyzer.MinimumTradeCount; }
		}

		public int BacklogCount
		{
			get { return ConfigManager.Config.DataAnalyzer.BacklogCount; }
		}

		public int SignalConfirmationCount
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

			Analyzer.Result[] results = new Analyzer.Result[Analyzers.Length];

			StringBuilder query = new StringBuilder();
			for (int i = 0; i < stocksTable.Rows.Count; ++i)
			{
				++totalProcessedCount;

				DataRow row = stocksTable.Rows[i];

				int id = Convert.ToInt32(row["id"]);

				DataTable historyTable = Data.QueryDataTable("SELECT take_time, count, volume, value, open, first, high, low, last, close, ((high-low)/2) median FROM snapshots WHERE stock_id=@stock_id AND DATE(take_time)<=DATE(@current_date) ORDER BY take_time",
					"stock_id", id,
					"current_date", CurrentDateTime);

				if (historyTable.Rows.Count == 0)
					continue;

				DataRow lastRow = historyTable.Rows[historyTable.Rows.Count - 1];

				if (Convert.ToInt32(lastRow["count"]) < MinimumTradeCount || Convert.ToDateTime(lastRow["take_time"]).Date != CurrentDateTime)
					continue;

				Analyzer.Info info = new Analyzer.Info { DateTime = CurrentDateTime, ID = id, Symbol = row["symbol"].ToString(), HistoryData = historyTable };

				for (int j = 0; j < Analyzers.Length; ++j)
				{
					var analyzer = Analyzers[j];

					results[j] = analyzer(info);
				}

				double buyWorthiness = 0;
				int confirmedBuySignalCount = 0;
				float buySignalPower = 0;
				FindSignal(results, BacklogCount, 1, out buyWorthiness, out confirmedBuySignalCount, out buySignalPower);

				double sellWorthiness = 0;
				int confirmedSellSignalCount = 0;
				float sellSignalPower = 0;
				FindSignal(results, BacklogCount, -1, out sellWorthiness, out confirmedSellSignalCount, out sellSignalPower);

				Debug.Assert(buySignalPower == 0 || buySignalPower != sellSignalPower);

				int confirmedSignalCount = 0;
				double worthiness = 0;

				if (buySignalPower > sellSignalPower)
				{
					confirmedSignalCount = confirmedBuySignalCount;
					worthiness = buyWorthiness;
				}
				else
				{
					confirmedSignalCount = confirmedSellSignalCount;
					worthiness = sellWorthiness;
				}

				if (confirmedSignalCount >= ConfigManager.Config.DataAnalyzer.SignalConfirmationCount)
				{
					worthiness /= (confirmedSignalCount + 1);

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

				if (ConfigManager.Config.DataAnalyzer.WriteToFile)
				{
					for (int j = 0; j < results.Length; ++j)
					{
						Analyzer.Result result = results[j];

						if (result == null)
							continue;

						int startIndex = historyTable.Rows.Count - result.Data.Rows.Count;

						for (int k = 0; k < result.Data.Columns.Count; ++k)
						{
							DataColumn column = result.Data.Columns[k];

							historyTable.Columns.Add(column.ColumnName, column.DataType);

							for (int l = 0; l < result.Data.Rows.Count; ++l)
								historyTable.Rows[startIndex + l][column.ColumnName] = result.Data.Rows[l][column.ColumnName];
						}
					}

					WriteCSV(ConfigManager.Config.DataAnalyzer.Path, info, historyTable);
				}

				int percent = (int)(totalProcessedCount / (float)stocksTable.Rows.Count * 100);
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

		private static void FindSignal(Analyzer.Result[] Results, int BacklogCount, int Action, out double Worthiness, out int ConfirmedSignalCount, out float SignalPower)
		{
			Worthiness = 0;
			ConfirmedSignalCount = 0;
			SignalPower = 0;

			int lastSingalIndex = BacklogCount - 1;

			int[] signalIndex = new int[Results.Length];
			for (int i = 0; i < signalIndex.Length; ++i)
				signalIndex[i] = -1;

			for (int i = 0; i < Results.Length; ++i)
			{
				if (Results[i] == null || signalIndex[i] != -1)
					continue;

				Analyzer.Signal signal = Results[i].Signals[lastSingalIndex];

				if (signal.Action == 0)
					continue;

				signalIndex[i] = i;

				if (signal.Action != Action)
					continue;

				Worthiness += signal.Action * signal.Worthiness;
				SignalPower += 1;

				for (int j = 0; j < Results.Length; ++j)
				{
					if (i == j)
						continue;

					if (Results[j] == null || signalIndex[j] != -1)
						continue;

					for (int l = lastSingalIndex; l > -1; --l)
					{
						Analyzer.Signal refSignal = Results[j].Signals[l];

						if (refSignal.Action == 0)
							continue;

						signalIndex[j] = j;

						if (refSignal.Action != Action)
							break;

						Worthiness += refSignal.Action * refSignal.Worthiness;
						++ConfirmedSignalCount;
						SignalPower += (l + 1) / (float)BacklogCount;

						break;
					}
				}
			}

			if (ConfirmedSignalCount == 0)
			{
				SignalPower = 0;
				return;
			}

			SignalPower /= Results.Length;
		}

		private static void WriteCSV(string Dir, Analyzer.Info Info, DataTable Data)
		{
			StringBuilder builder = new StringBuilder();
			CSVWriter.Write(builder, 0, 0, Data);

			Helper.WriteToFile(Dir, Info.DateTime, Info.ID + "_" + Info.Symbol + ".csv", builder.ToString());
		}
	}
}