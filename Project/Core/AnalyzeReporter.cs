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
			DataTable analyzesData = Data.QueryDataTable("SELECT s.id stock_id, s.name, s.symbol, DATE(analyze_time) analyze_time, a.action, a.worthiness FROM analyzes a INNER JOIN stocks s ON a.stock_id=s.id WHERE DATE(a.analyze_time)=DATE(@time) ORDER BY a.worthiness DESC", "time", CurrentDateTime);
			if (analyzesData.Rows.Count == 0)
				return true;

			DataTable tradersData = Data.QueryDataTable("SELECT id, name, emails, send_full_sell_report FROM traders");
			DataTable tradesData = Data.QueryDataTable("SELECT trader_id, stock_id, SUM(count * action) count FROM trades WHERE DATE(action_time)<=DATE(@time) GROUP BY trader_id, stock_id", "time", CurrentDateTime);
			
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

					WriteTableRow(fullBuyText, i + 1, row["name"].ToString(), row["symbol"].ToString(), Convert.ToDouble(row["worthiness"]));
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

					WriteTableRow(fullSellText, i + 1, row["name"].ToString(), row["symbol"].ToString(), Convert.ToDouble(row["worthiness"]));
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

								WriteTableRow(emailBody, ++sellCount, sellRow["name"].ToString(), sellRow["symbol"].ToString(), Convert.ToDouble(sellRow["worthiness"]));
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

				Helper.SendEmail(string.Format("Suggested Trades on {0}", actionTime.ToPersianDate()), body, emailsArr, name);
			}

			return true;
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
			HTMLGenerator.WriteContent(Builder, "Worthiness(%)");
			HTMLGenerator.EndTableData(Builder);

			HTMLGenerator.EndTableRow(Builder);
			HTMLGenerator.EndTableHeader(Builder);
		}

		private static void WriteTableRow(StringBuilder Builder, int Number, string Name, string Symbol, double Worthiness)
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
			HTMLGenerator.WriteContent(Builder, (Worthiness * 100).ToString("N2"));
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.EndTableRow(Builder);
		}
	}
}