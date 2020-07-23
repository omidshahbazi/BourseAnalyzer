using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Core
{
	public class AnalyzeReporter : Worker
	{
		private const string SUBJECT_TEMPLATE = "Suggested trades on {0}";
		private const string BODY_TEMPLATE = "<html><body>{0}</body></html>";

		public override float WorkHour
		{
			get { return ConfigManager.Config.AnalyzeReporter.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			DateTime actionTime = CurrentDateTime.AddDays(1);
			if (actionTime.DayOfWeek == DayOfWeek.Thursday)
				actionTime = actionTime.AddDays(2);

			DataTable tradersData = Data.Database.QueryDataTable("SELECT id, name, email, send_full_sell_report FROM traders");
			DataTable tradesData = Data.Database.QueryDataTable("SELECT trader_id, stock_id, SUM(count * action) count FROM trades WHERE DATE(action_time)<=DATE(@time) GROUP BY trader_id, stock_id", "time", CurrentDateTime);
			DataTable analyzeData = Data.Database.QueryDataTable("SELECT s.id stock_id, s.name, s.symbol, a.action, a.worthiness FROM analyzes a INNER JOIN stocks s ON a.stock_id=s.id WHERE DATE(analyze_time)=DATE(@time)", "time", CurrentDateTime);
			analyzeData.DefaultView.Sort = "worthiness DESC";

			analyzeData.DefaultView.RowFilter = "action=1";
			DataTable buyAnalyzeData = analyzeData.DefaultView.ToTable();

			analyzeData.DefaultView.RowFilter = "action=-1";
			DataTable sellAnalyzeData = analyzeData.DefaultView.ToTable();

			StringBuilder fullBuyText = new StringBuilder();
			{
				HTMLGenerator.BeginHeader2(fullBuyText, Color.Green);
				HTMLGenerator.WriteContent(fullBuyText, "Full Buy Report:");
				HTMLGenerator.EndHeader2(fullBuyText);

				BeginTable(fullBuyText);

				for (int i = 0; i < buyAnalyzeData.Rows.Count; ++i)
				{
					DataRow row = buyAnalyzeData.Rows[i];

					HTMLGenerator.BeginTableRow(fullBuyText);
					HTMLGenerator.BeginTableData(fullBuyText);
					HTMLGenerator.WriteContent(fullBuyText, i + 1);
					HTMLGenerator.EndTableData(fullBuyText);
					HTMLGenerator.BeginTableData(fullBuyText);
					HTMLGenerator.WriteContent(fullBuyText, row["name"]);
					HTMLGenerator.EndTableData(fullBuyText);
					HTMLGenerator.BeginTableData(fullBuyText);
					HTMLGenerator.WriteContent(fullBuyText, row["symbol"]);
					HTMLGenerator.EndTableData(fullBuyText);
					HTMLGenerator.BeginTableData(fullBuyText);
					HTMLGenerator.WriteContent(fullBuyText, (int)(Convert.ToDouble(row["worthiness"]) * 100) + "%");
					HTMLGenerator.EndTableData(fullBuyText);
					HTMLGenerator.EndTableRow(fullBuyText);
				}

				HTMLGenerator.EndTable(fullBuyText);
			}

			StringBuilder fullSellText = new StringBuilder();
			{
				HTMLGenerator.BeginHeader2(fullSellText, Color.Red);
				HTMLGenerator.WriteContent(fullSellText, "Full Sell Report:");
				HTMLGenerator.EndHeader2(fullSellText);

				BeginTable(fullSellText);

				for (int i = 0; i < sellAnalyzeData.Rows.Count; ++i)
				{
					DataRow row = sellAnalyzeData.Rows[i];

					HTMLGenerator.BeginTableRow(fullSellText);
					HTMLGenerator.BeginTableData(fullSellText);
					HTMLGenerator.WriteContent(fullSellText, i + 1);
					HTMLGenerator.EndTableData(fullSellText);
					HTMLGenerator.BeginTableData(fullSellText);
					HTMLGenerator.WriteContent(fullSellText, row["name"]);
					HTMLGenerator.EndTableData(fullSellText);
					HTMLGenerator.BeginTableData(fullSellText);
					HTMLGenerator.WriteContent(fullSellText, row["symbol"]);
					HTMLGenerator.EndTableData(fullSellText);
					HTMLGenerator.BeginTableData(fullSellText);
					HTMLGenerator.WriteContent(fullSellText, (int)(Convert.ToDouble(row["worthiness"]) * 100) + "%");
					HTMLGenerator.EndTableData(fullSellText);
					HTMLGenerator.EndTableRow(fullSellText);
				}

				HTMLGenerator.EndTable(fullBuyText);
			}

			for (int i = 0; i < tradersData.Rows.Count; ++i)
			{
				StringBuilder emailBody = new StringBuilder();

				HTMLGenerator.BeginHeader2(emailBody, Color.Blue);
				HTMLGenerator.WriteContent(emailBody, "Suggested trades on {0}", actionTime.ToPersianDateTime());
				HTMLGenerator.EndHeader2(emailBody);

				DataRow traderRow = tradersData.Rows[i];

				string emailAddress = traderRow["email"].ToString();
				if (string.IsNullOrEmpty(emailAddress))
					continue;

				int sellCount = 0;

				tradesData.DefaultView.RowFilter = "trader_id=" + Convert.ToInt32(traderRow["id"]);
				if (tradesData.DefaultView.Count != 0)
				{
					HTMLGenerator.BeginHeader2(emailBody, Color.Red);
					HTMLGenerator.WriteContent(emailBody, "Should Sell:");
					HTMLGenerator.EndHeader2(emailBody);

					{
						BeginTable(emailBody);

						for (int j = 0; j < tradesData.DefaultView.Count; ++j)
						{
							DataRowView tradeRow = tradesData.DefaultView[j];

							if (Convert.ToInt32(tradeRow["count"]) <= 0)
								continue;

							sellAnalyzeData.DefaultView.RowFilter = "stock_id=" + tradeRow["stock_id"];
							if (sellAnalyzeData.DefaultView.Count != 0)
							{
								DataRowView sellRow = sellAnalyzeData.DefaultView[0];

								HTMLGenerator.BeginTableRow(emailBody);
								HTMLGenerator.BeginTableData(emailBody);
								HTMLGenerator.WriteContent(emailBody, ++sellCount);
								HTMLGenerator.EndTableData(emailBody);
								HTMLGenerator.BeginTableData(emailBody);
								HTMLGenerator.WriteContent(emailBody, sellRow["name"]);
								HTMLGenerator.EndTableData(emailBody);
								HTMLGenerator.BeginTableData(emailBody);
								HTMLGenerator.WriteContent(emailBody, sellRow["symbol"]);
								HTMLGenerator.EndTableData(emailBody);
								HTMLGenerator.BeginTableData(emailBody);
								HTMLGenerator.WriteContent(emailBody, (int)(Convert.ToDouble(sellRow["worthiness"]) * 100) + "%");
								HTMLGenerator.EndTableData(emailBody);
								HTMLGenerator.EndTableRow(emailBody);
							}
						}

						HTMLGenerator.EndTable(emailBody);
					}
				}

				string body = emailBody.ToString() + fullBuyText.ToString();

				if (Convert.ToBoolean(traderRow["send_full_sell_report"]))
					body += fullSellText.ToString();

				SendEmail(traderRow["name"].ToString(), emailAddress, body, actionTime);
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
			HTMLGenerator.WriteContent(Builder, "Worthiness");
			HTMLGenerator.EndTableData(Builder);
			HTMLGenerator.EndTableRow(Builder);
			HTMLGenerator.EndTableHeader(Builder);
		}

		private static bool SendEmail(string Name, string Email, string HTMLBody, DateTime Date)
		{
			MailMessage message = new MailMessage();
			message.From = new MailAddress(ConfigManager.Config.AnalyzeReporter.Username, "Bourse Analyzer");
			message.To.Add(new MailAddress(Email, Name));

			message.Subject = string.Format(SUBJECT_TEMPLATE, Date.ToPersianDateTime());
			message.Body = string.Format(BODY_TEMPLATE, HTMLBody);

			message.IsBodyHtml = true;

			SmtpClient client = new SmtpClient(ConfigManager.Config.AnalyzeReporter.Host, ConfigManager.Config.AnalyzeReporter.Port);
			client.UseDefaultCredentials = false;
			client.EnableSsl = true;
			client.Credentials = new NetworkCredential(ConfigManager.Config.AnalyzeReporter.Username, ConfigManager.Config.AnalyzeReporter.Password);

			try
			{
				client.Send(message);

				return true;
			}
			catch (Exception e)
			{ }

			return false;
		}
	}
}