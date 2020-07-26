using GameFramework.ASCIISerializer;
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

			DataTable tradersData = Data.QueryDataTable("SELECT id, name, emails, send_full_sell_report FROM traders");
			DataTable tradesData = Data.QueryDataTable("SELECT trader_id, stock_id, SUM(count * action) count FROM trades WHERE DATE(action_time)<=DATE(@time) GROUP BY trader_id, stock_id", "time", CurrentDateTime);
			DataTable analyzeData = Data.QueryDataTable("SELECT s.id stock_id, s.name, s.symbol, a.action, a.worthiness FROM analyzes a INNER JOIN stocks s ON a.stock_id=s.id WHERE DATE(analyze_time)=DATE(@time)", "time", CurrentDateTime);
			analyzeData.DefaultView.Sort = "worthiness DESC";

			analyzeData.DefaultView.RowFilter = "action=1";
			DataTable buyAnalyzeData = analyzeData.DefaultView.ToTable();

			analyzeData.DefaultView.RowFilter = "action=-1";
			DataTable sellAnalyzeData = analyzeData.DefaultView.ToTable();

			HTMLGenerator.Style = new HTMLGenerator.HTMLStyle();

			Font font = new Font("Calibri", 18);

			StringBuilder fullBuyText = new StringBuilder();
			{
				HTMLGenerator.Style.Color = Color.Green;
				HTMLGenerator.Style.Font = null;
				HTMLGenerator.BeginHeader2(fullBuyText);
				HTMLGenerator.Style.Color = Color.Black;
				HTMLGenerator.WriteContent(fullBuyText, "Full Buy Report:");
				HTMLGenerator.EndHeader2(fullBuyText);

				HTMLGenerator.Style.Font = font;
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
				HTMLGenerator.Style.Font = null;
			}

			StringBuilder fullSellText = new StringBuilder();
			{
				HTMLGenerator.Style.Color = Color.Red;
				HTMLGenerator.BeginHeader2(fullSellText);
				HTMLGenerator.Style.Color = Color.Black;
				HTMLGenerator.WriteContent(fullSellText, "Full Sell Report:");
				HTMLGenerator.EndHeader2(fullSellText);

				HTMLGenerator.Style.Font = font;
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
				HTMLGenerator.Style.Font = null;
			}

			for (int i = 0; i < tradersData.Rows.Count; ++i)
			{
				StringBuilder emailBody = new StringBuilder();

				DataRow traderRow = tradersData.Rows[i];

				string name = traderRow["name"].ToString();

				ISerializeArray emailsArr = Creator.Create<ISerializeArray>(traderRow["emails"].ToString());
				if (emailsArr == null || emailsArr.Count == 0)
					continue;

				HTMLGenerator.BeginHeader2(emailBody);
				HTMLGenerator.WriteContent(emailBody, "Hi {0}!", name);
				HTMLGenerator.EndHeader2(emailBody);

				HTMLGenerator.Style.Color = Color.Blue;
				HTMLGenerator.BeginHeader2(emailBody);
				HTMLGenerator.Style.Color = Color.Black;
				HTMLGenerator.WriteContent(emailBody, "Suggested trades on {0}", actionTime.ToPersianDateTime());
				HTMLGenerator.EndHeader2(emailBody);

				tradesData.DefaultView.RowFilter = "trader_id=" + Convert.ToInt32(traderRow["id"]);
				if (tradesData.DefaultView.Count != 0)
				{
					HTMLGenerator.Style.Color = Color.Red;
					HTMLGenerator.BeginHeader2(emailBody);
					HTMLGenerator.Style.Color = Color.Black;
					HTMLGenerator.WriteContent(emailBody, "Should Sell:");
					HTMLGenerator.EndHeader2(emailBody);

					{
						HTMLGenerator.Style.Font = font;
						BeginTable(emailBody);

						int sellCount = 0;
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
						HTMLGenerator.Style.Font = null;
					}
				}

				string body = emailBody.ToString() + fullBuyText.ToString();

				if (Convert.ToBoolean(traderRow["send_full_sell_report"]))
					body += fullSellText.ToString();

				SendEmail(name, emailsArr, body, actionTime);
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

		private static bool SendEmail(string Name, ISerializeArray EmailsArray, string HTMLBody, DateTime Date)
		{
			MailMessage message = new MailMessage();
			message.From = new MailAddress(ConfigManager.Config.AnalyzeReporter.Username, "Bourse Analyzer");

			message.To.Add(new MailAddress(EmailsArray.Get<string>(0), Name));
			for (uint i = 1; i < EmailsArray.Count; ++i)
				message.CC.Add(new MailAddress(EmailsArray.Get<string>(i), Name));

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