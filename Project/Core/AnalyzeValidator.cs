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


			DateTime startTime = CurrentDateTime.Date.AddDays(-PreviousAnalyzes);
			if (startTime.DayOfWeek == DayOfWeek.Friday)
				startTime = startTime.AddDays(-2);

			DataTable analyzesData = Data.QueryDataTable("SELECT id, stock_id, action FROM analyzes WHERE DATE(analyze_time)=DATE(@date)", "date", startTime);
			DataTable snapshotsData = Data.QueryDataTable("SELECT stock_id, close FROM snapshots WHERE DATE(take_time) IN(DATE(@start_time), DATE(@date)) ORDER BY take_time", "start_time", startTime, "date", CurrentDateTime);

			StringBuilder query = new StringBuilder();

			for (int i = 0; i < analyzesData.Rows.Count; ++i)
			{
				DataRow analyzeRow = analyzesData.Rows[i];

				snapshotsData.DefaultView.RowFilter = string.Format("stock_id={0}", analyzeRow["stock_id"]);
				if (snapshotsData.DefaultView.Count < 1)
					continue;

				int stockTrendeSign = Math.Sign(Convert.ToInt32(snapshotsData.DefaultView[snapshotsData.DefaultView.Count - 1]["close"]) - Convert.ToInt32(snapshotsData.DefaultView[0]["close"]));

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