using System;
using System.Data;
using System.Text;

namespace Core
{
	public class ValidationReporter : Worker
	{
		public override bool Enabled
		{
			get { return ConfigManager.Config.ValidationReporter.Enabled; }
		}

		public override float WorkHour
		{
			get { return ConfigManager.Config.ValidationReporter.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			DateTime startTime = CurrentDateTime.Date.AddDays(-1);
			if (CurrentDateTime.DayOfWeek == DayOfWeek.Friday)
				startTime = startTime.AddDays(-2);

			DataTable analyzesData = Data.QueryDataTable("SELECT s.name, s.symbol, a.action, v.was_valid FROM analyzes a INNER JOIN stocks s ON a.stock_id=s.id INNER JOIN analyzes_validation v ON a.id=v.analyze_id WHERE DATE(a.analyze_time)=DATE(@date)", "date", startTime);
			DataTable snapshotsData = Data.QueryDataTable("SELECT stock_id, open, close FROM snapshots WHERE DATE(take_time)=DATE(UTC_TIMESTAMP())");

			StringBuilder query = new StringBuilder();

			for (int i = 0; i < analyzesData.Rows.Count; ++i)
			{
				DataRow analyzeRow = analyzesData.Rows[i];

				snapshotsData.DefaultView.RowFilter = string.Format("stock_id={0}", analyzeRow["stock_id"]);
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
				Data.Execute(query.ToString());

			return true;
		}
	}
}