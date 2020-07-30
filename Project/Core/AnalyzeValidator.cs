using System;
using System.Data;
using System.Text;

namespace Core
{
	public class AnalyzeValidator : Worker
	{
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
			DateTime analyzeTime = CurrentDateTime.Date.AddDays(-1);
			if (analyzeTime.DayOfWeek == DayOfWeek.Friday)
				analyzeTime = analyzeTime.AddDays(-1);
			if (analyzeTime.DayOfWeek == DayOfWeek.Thursday)
				analyzeTime = analyzeTime.AddDays(-1);

			DataTable analyzesData = Data.QueryDataTable("SELECT id, stock_id, action FROM analyzes WHERE DATE(analyze_time)=DATE(@time)", "time", analyzeTime);
			DataTable snapshotsData = Data.QueryDataTable("SELECT stock_id, DATE(take_time) take_time, close FROM snapshots WHERE DATE(take_time)<=DATE(@analyze_time) ORDER BY take_time", "analyze_time", analyzeTime);

			StringBuilder query = new StringBuilder();

			for (int i = 0; i < analyzesData.Rows.Count; ++i)
			{
				DataRow analyzeRow = analyzesData.Rows[i];

				snapshotsData.DefaultView.RowFilter = string.Format("stock_id={0}", analyzeRow["stock_id"]);
				DataTable smaData = Analyzer.GenerateSimpleMovingAverageData(snapshotsData.DefaultView.ToTable(), "close", 9, 2);
				if (smaData == null)
					continue;

				int actionSign = Math.Sign(Convert.ToInt32(analyzeRow["action"]));
				int stockTrendeSign = Math.Sign(Convert.ToInt32(smaData.Rows[smaData.Rows.Count - 1]["sma"]) - Convert.ToInt32(smaData.Rows[smaData.Rows.Count - 2]["sma"]));

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