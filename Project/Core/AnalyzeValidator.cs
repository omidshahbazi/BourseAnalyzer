using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Text;

namespace Core
{
	public class AnalyzeValidator : Worker
	{
		protected override float WorkHour
		{
			get { return ConfigManager.Config.AnalyzeValidator.WorkHour; }
		}

		protected override bool Do()
		{
			ConsoleHelper.WriteInfo("Validaing analyzes...");

			DataTable startDateData = Data.Database.QueryDataTable("SELECT TIMESTAMPADD(DAY, IF(DAYOFWEEK(NOW())=7, -3, -1), DATE(NOW())) start_date");
			DateTime startDate = Convert.ToDateTime(startDateData.Rows[0]["start_date"]);

			DataTable analyzesData = Data.Database.QueryDataTable("SELECT id, stock_id, action FROM analyzes WHERE DATE(analyze_time)=@date", "date", startDate);
			DataTable snapshotsData = Data.Database.QueryDataTable("SELECT stock_id, open, close FROM snapshots WHERE DATE(take_time)=DATE(NOW())");

			StringBuilder query = new StringBuilder();

			for (int i = 0; i < analyzesData.Rows.Count; ++i)
			{
				DataRow analyzeRow = analyzesData.Rows[i];

				snapshotsData.DefaultView.RowFilter = "stock_id=" + analyzeRow["stock_id"];
				if (snapshotsData.DefaultView.Count == 0)
					continue;

				int actionSign = Math.Sign(Convert.ToInt32(analyzeRow["action"]));

				DataRowView snapshotRow = snapshotsData.DefaultView[0];
				int stockTrendSign = Math.Sign(Convert.ToInt32(snapshotRow["close"]) - Convert.ToInt32(snapshotRow["open"]));

				query.Append("INSERT INTO analyzes_validation(analyze_id, was_valid) VALUES(");
				query.Append(analyzeRow["id"]);
				query.Append(',');
				query.Append(actionSign == stockTrendSign);
				query.Append(");");
			}

			if (query.Length != 0)
				Data.Database.Execute(query.ToString());

			ConsoleHelper.WriteInfo("Validating analyzes done");

			return true;
		}
	}
}