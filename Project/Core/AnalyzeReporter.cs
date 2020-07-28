using GameFramework.ASCIISerializer;
using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Drawing;
using System.Text;

namespace Core
{
	public class AnalyzeReporter : Worker
	{
		private const string BODY_TEMPLATE = "<html><body>{0}</body></html>";

		public override bool Enabled
		{
			get { return ConfigManager.Config.AnalyzeReporter.Enabled; }
		}

		public override float WorkHour
		{
			get { return ConfigManager.Config.AnalyzeReporter.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			DataTable tradersData = Data.QueryDataTable("SELECT id, name, emails, send_full_sell_report FROM traders");
			DataTable tradesData = Data.QueryDataTable("SELECT trader_id, stock_id, SUM(count * action) count FROM trades WHERE DATE(action_time)<=DATE(@time) GROUP BY trader_id, stock_id", "time", CurrentDateTime);
			DataTable analyzesData = Data.QueryDataTable("SELECT s.id stock_id, s.name, s.symbol, DATE(analyze_time) analyze_time, a.action, a.worthiness FROM analyzes a INNER JOIN stocks s ON a.stock_id=s.id WHERE DATE(a.analyze_time)<=DATE(@time) ORDER BY a.worthiness DESC", "time", CurrentDateTime);
			DataTable snapshotsData = Data.QueryDataTable("SELECT stock_id, DATE(take_time) take_time, close FROM snapshots WHERE DATE(take_time)<=DATE(@time)", "time", CurrentDateTime);

			analyzesData.DefaultView.RowFilter = string.Format("action=1 AND analyze_time='{0}'", CurrentDateTime.Date.ToDatabaseDateTime());
			DataTable buyAnalyzeData = analyzesData.DefaultView.ToTable();

			analyzesData.DefaultView.RowFilter = string.Format("action=-1 AND analyze_time='{0}'", CurrentDateTime.Date.ToDatabaseDateTime());
			DataTable sellAnalyzeData = analyzesData.DefaultView.ToTable();

			HTMLGenerator.Style = new HTMLGenerator.HTMLStyle();

			Font font = new Font("Calibri", 18);

			StringBuilder fullBuyText = new StringBuilder();
			{
				HTMLGenerator.Style.Font = null;
				Helper.HTML_WriteHeader2(fullBuyText, "Full Buy Report:", Color.Green);

				HTMLGenerator.Style.Font = font;
				BeginTable(fullBuyText);

				for (int i = 0; i < buyAnalyzeData.Rows.Count; ++i)
				{
					DataRow row = buyAnalyzeData.Rows[i];

					double changes = CalculateChanges(analyzesData, snapshotsData, -1, Convert.ToInt32(row["stock_id"]), CurrentDateTime);

					WriteTableRow(fullBuyText, i + 1, row["name"].ToString(), row["symbol"].ToString(), changes, Convert.ToDouble(row["worthiness"]));
				}

				HTMLGenerator.EndTable(fullBuyText);
				HTMLGenerator.Style.Font = null;
			}

			StringBuilder fullSellText = new StringBuilder();
			{
				HTMLGenerator.Style.Font = null;
				Helper.HTML_WriteHeader2(fullSellText, "Full Sell Report:", Color.Red);

				HTMLGenerator.Style.Font = font;
				BeginTable(fullSellText);

				for (int i = 0; i < sellAnalyzeData.Rows.Count; ++i)
				{
					DataRow row = sellAnalyzeData.Rows[i];

					double changes = CalculateChanges(analyzesData, snapshotsData, 1, Convert.ToInt32(row["stock_id"]), CurrentDateTime);

					WriteTableRow(fullSellText, i + 1, row["name"].ToString(), row["symbol"].ToString(), changes, Convert.ToDouble(row["worthiness"]));
				}

				HTMLGenerator.EndTable(fullBuyText);
				HTMLGenerator.Style.Font = null;
			}

			DateTime actionTime = CurrentDateTime.AddDays(1);
			if (actionTime.DayOfWeek == DayOfWeek.Thursday)
				actionTime = actionTime.AddDays(2);

			for (int i = 0; i < tradersData.Rows.Count; ++i)
			{
				StringBuilder emailBody = new StringBuilder();

				DataRow traderRow = tradersData.Rows[i];

				string name = traderRow["name"].ToString();

				ISerializeArray emailsArr = Creator.Create<ISerializeArray>(traderRow["emails"].ToString());
				if (emailsArr == null || emailsArr.Count == 0)
					continue;

				HTMLGenerator.Style.Font = null;
				Helper.HTML_WriteHeader2(emailBody, string.Format("Hi {0}!", name), Color.Black);

				HTMLGenerator.Style.Font = null;
				Helper.HTML_WriteHeader2(emailBody, string.Format("Suggested Trades on {0}", actionTime.ToPersianDate()), Color.Blue);

				tradesData.DefaultView.RowFilter = string.Format("trader_id={0}", Convert.ToInt32(traderRow["id"]));
				if (tradesData.DefaultView.Count != 0)
				{
					HTMLGenerator.Style.Font = null;
					Helper.HTML_WriteHeader2(emailBody, "Should Sell:", Color.Red);

					{
						HTMLGenerator.Style.Font = font;
						BeginTable(emailBody);

						int sellCount = 0;
						for (int j = 0; j < tradesData.DefaultView.Count; ++j)
						{
							DataRowView tradeRow = tradesData.DefaultView[j];

							if (Convert.ToInt32(tradeRow["count"]) <= 0)
								continue;

							sellAnalyzeData.DefaultView.RowFilter = string.Format("stock_id={0}", tradeRow["stock_id"]);
							if (sellAnalyzeData.DefaultView.Count != 0)
							{
								DataRowView sellRow = sellAnalyzeData.DefaultView[0];

								double changes = CalculateChanges(analyzesData, snapshotsData, 1, Convert.ToInt32(sellRow["stock_id"]), CurrentDateTime);

								WriteTableRow(emailBody, ++sellCount, sellRow["name"].ToString(), sellRow["symbol"].ToString(), changes, Convert.ToDouble(sellRow["worthiness"]));
							}
						}

						HTMLGenerator.EndTable(emailBody);
						HTMLGenerator.Style.Font = null;
					}
				}

				string body = emailBody.ToString() + fullBuyText.ToString();

				if (Convert.ToBoolean(traderRow["send_full_sell_report"]))
					body += fullSellText.ToString();

				body = string.Format(BODY_TEMPLATE, body);

				if (ConfigManager.Config.AnalyzeReporter.WriteToFile)
					Helper.WriteToFile(ConfigManager.Config.AnalyzeReporter.Path, CurrentDateTime, name + ".html", body);

				return Helper.SendEmail(string.Format("Suggested Trades on {0}", actionTime.ToPersianDate()), body, emailsArr, name);
			}

			return true;
		}

		private static double CalculateChanges(DataTable AnalyzesData, DataTable SnapshotsData, int PreviousAction, int StockID, DateTime CurrentDate)
		{
			AnalyzesData.DefaultView.RowFilter = string.Format("stock_id={0} AND action={1} AND analyze_time<'{2}'", StockID, PreviousAction, CurrentDate.Date.ToDatabaseDateTime());
			if (AnalyzesData.DefaultView.Count != 0)
				return Helper.CalculateChanges(SnapshotsData, StockID, Convert.ToDateTime(AnalyzesData.DefaultView[0]["analyze_time"]), CurrentDate);

			return double.NaN;
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
			HTMLGenerator.WriteContent(Builder, "Changes(%)");
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, "Worthiness(%)");
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.EndTableRow(Builder);
			HTMLGenerator.EndTableHeader(Builder);
		}

		private static void WriteTableRow(StringBuilder Builder, int Number, string Name, string Symbol, double Changes, double Worthiness)
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
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, double.IsNaN(Changes) ? "N/A" : ((int)(Changes * 100)).ToString());
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.BeginTableData(Builder);
			HTMLGenerator.WriteContent(Builder, (int)(Worthiness * 100));
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.EndTableRow(Builder);
		}
	}
}