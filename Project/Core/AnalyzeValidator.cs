using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Text;

namespace Core
{
	public class AnalyzeValidator : Worker
	{
		public int PreviousAnalyzes
		{
			get { return ConfigManager.Config.AnalyzeValidator.PreviousAnalyzes; }
		}

		public int DayGapCount
		{
			get { return ConfigManager.Config.AnalyzeValidator.DayGapCount; }
		}

		public override bool Enabled
		{
			get { return ConfigManager.Config.AnalyzeValidator.Enabled; }
		}

		public override float WorkHour
		{
			get { return ConfigManager.Config.AnalyzeValidator.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			if (PreviousAnalyzes < 1)
			{
				ConsoleHelper.WriteError("PreviousAnalyzes must be grater than 1, current value is {0}", PreviousAnalyzes);
				return false;
			}

			DateTime analyzeTime = CurrentDateTime.Date.AddDays(-PreviousAnalyzes);
			if (analyzeTime.DayOfWeek == DayOfWeek.Friday)
				analyzeTime = analyzeTime.AddDays(-1);
			if (analyzeTime.DayOfWeek == DayOfWeek.Thursday)
				analyzeTime = analyzeTime.AddDays(-1);

			DataTable analyzesData = Data.QueryDataTable("SELECT id, stock_id, action FROM analyzes WHERE DATE(analyze_time)=DATE(@time)", "time", analyzeTime);
			DataTable snapshotsData = Data.QueryDataTable("SELECT stock_id, DATE(take_time) take_time, close FROM snapshots WHERE DATE(take_time)>=DATE(@analyze_time) ORDER BY take_time", "analyze_time", analyzeTime);

			StringBuilder query = new StringBuilder();

			for (int i = 0; i < analyzesData.Rows.Count; ++i)
			{
				DataRow analyzeRow = analyzesData.Rows[i];

				snapshotsData.DefaultView.RowFilter = string.Format("stock_id={0}", analyzeRow["stock_id"]);
				if (snapshotsData.DefaultView.Count < PreviousAnalyzes + 1)
					continue;

				DataRowView analyzeDateSnapshotRow = snapshotsData.DefaultView[0];
				DataRowView afterAnalyzeDateSnapshotRow = snapshotsData.DefaultView[PreviousAnalyzes];

				if ((Convert.ToDateTime(afterAnalyzeDateSnapshotRow["take_time"]) - Convert.ToDateTime(analyzeDateSnapshotRow["take_time"])).Days > PreviousAnalyzes + DayGapCount)
					continue;

				int stockTrendeSign = Math.Sign(Convert.ToInt32(afterAnalyzeDateSnapshotRow["close"]) - Convert.ToInt32(analyzeDateSnapshotRow["close"]));

				int actionSign = Math.Sign(Convert.ToInt32(analyzeRow["action"]));

				query.Append("INSERT INTO analyzes_validation(analyze_id, was_valid) VALUES(");
				query.Append(analyzeRow["id"]);
				query.Append(',');
				query.Append(actionSign == stockTrendeSign);
				query.Append(");");
			}

			if (query.Length != 0)
				Data.Execute(query.ToString());

			return true;
		}
	}
}