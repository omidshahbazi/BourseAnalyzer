using GameFramework.ASCIISerializer;
using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Drawing;
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

			DataTable analyzesData = Data.QueryDataTable("SELECT a.stock_id, s.name, s.symbol, a.action, DATE(a.analyze_time) analyze_time, v.was_valid FROM analyzes a INNER JOIN stocks s ON a.stock_id=s.id LEFT OUTER JOIN analyzes_validation v ON a.id=v.analyze_id WHERE DATE(a.analyze_time)=DATE(@date) ORDER BY a.worthiness DESC", "date", startTime);
			DataTable snapshotsData = Data.QueryDataTable("SELECT stock_id, DATE(take_time) take_time, close FROM snapshots WHERE DATE(take_time)<=DATE(@time)", "time", CurrentDateTime);
			DataTable tradersData = Data.QueryDataTable("SELECT id, name, emails, send_full_sell_report FROM traders WHERE is_admin=1");

			StringBuilder bodyBuilder = new StringBuilder();

			HTMLGenerator.BeginHTML(bodyBuilder);
			HTMLGenerator.BeginBody(bodyBuilder);

			HTMLGenerator.Style = new HTMLGenerator.HTMLStyle();
			HTMLGenerator.Style.Font = new Font("calibri", 18);

			Helper.HTML_WriteHeader2(bodyBuilder, "Analyze Validation Report", Color.Black);
			Helper.HTML_WriteHeader2(bodyBuilder, string.Format("Analyze Date: {0}", startTime.ToPersianDate()), Color.Black);

			analyzesData.DefaultView.RowFilter = "was_valid=0";
			WriteTable(bodyBuilder, "Invalid Analyzes", analyzesData.Rows.Count, Color.Red, analyzesData.DefaultView.ToTable(), snapshotsData, startTime, CurrentDateTime);

			analyzesData.DefaultView.RowFilter = "was_valid IS NULL";
			WriteTable(bodyBuilder, "Unknown Analyzes", analyzesData.Rows.Count, Color.Black, analyzesData.DefaultView.ToTable(), snapshotsData, startTime, CurrentDateTime);

			analyzesData.DefaultView.RowFilter = "was_valid=1";
			WriteTable(bodyBuilder, "Valid Analyzes", analyzesData.Rows.Count, Color.Green, analyzesData.DefaultView.ToTable(), snapshotsData, startTime, CurrentDateTime);

			HTMLGenerator.EndBody(bodyBuilder);
			HTMLGenerator.EndHTML(bodyBuilder);

			string body = bodyBuilder.ToString();

			if (ConfigManager.Config.ValidationReporter.WriteToFile)
				Helper.WriteToFile(ConfigManager.Config.ValidationReporter.Path, CurrentDateTime, startTime.ToString("yyyy-MM-dd") + ".html", body);

			for (int i = 0; i < tradersData.Rows.Count; ++i)
			{
				DataRow row = tradersData.Rows[i];

				ISerializeArray emailsArr = Creator.Create<ISerializeArray>(row["emails"].ToString());
				if (emailsArr == null || emailsArr.Count == 0)
					continue;

				Helper.SendEmail(string.Format("Validation Report on {0}", startTime.ToPersianDate()), body, emailsArr, row["name"].ToString());
			}

			return true;
		}

		private static void WriteTable(StringBuilder Builder, string Title, int AnalyzeCount, Color Color, DataTable Data, DataTable SnapshotsData, DateTime StartTime, DateTime CurrentTime)
		{
			Helper.HTML_WriteHeader2(Builder, string.Format("{0} {1}, {2}%", Title, Data.Rows.Count, (Data.Rows.Count / (float)AnalyzeCount) * 100), Color);
			BeginTable(Builder);
			for (int i = 0; i < Data.Rows.Count; ++i)
			{
				DataRow row = Data.Rows[i];

				double changes = Helper.CalculateChanges(SnapshotsData, Convert.ToInt32(row["stock_id"]), StartTime, CurrentTime);

				WriteTableRow(Builder, i + 1, row["name"].ToString(), row["symbol"].ToString(), Convert.ToInt32(row["action"]), changes);
			}

			HTMLGenerator.EndTable(Builder);
		}

		private static void BeginTable(StringBuilder Builder)
		{
			HTMLGenerator.BeginTable(Builder);
			HTMLGenerator.BeginTableHeader(Builder);
			HTMLGenerator.BeginTableRow(Builder);
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, "No.");
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, "Name");
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, "Symbol");
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, "Action");
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, "Changes(%)");
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.EndTableRow(Builder);
			HTMLGenerator.EndTableHeader(Builder);
		}

		private static void WriteTableRow(StringBuilder Builder, int Number, string Name, string Symbol, int Action, double Changes)
		{
			HTMLGenerator.BeginTableRow(Builder);

			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, Number);
			HTMLGenerator.EndTableData(Builder);

			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, Name);
			HTMLGenerator.EndTableData(Builder);

			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, Symbol);
			HTMLGenerator.EndTableData(Builder);

			HTMLGenerator.Style.Color = (Action == 1 ? Color.Green : Color.Red);
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, (Action == 1 ? "Buy" : "Sell"));
			HTMLGenerator.EndTableData(Builder);

			HTMLGenerator.Style.Color = (double.IsNaN(Changes) ? Color.Black : (Changes > 0 ? Color.Green : Color.Red));
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, double.IsNaN(Changes) ? "N/A" : (Changes * 100).ToString());
			HTMLGenerator.EndTableData(Builder);

			HTMLGenerator.EndTableRow(Builder);

			HTMLGenerator.Style.Color = Color.Black;
		}
	}
}