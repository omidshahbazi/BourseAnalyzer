using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Core
{
	public class AnlayzeReporter : Worker
	{
		private const string SUBJECT_TEMPLATE = "Suggested trades on {0}";
		private const string BODY_TEMPLATE = "Suggested trades on {0}\n\n\n{1}";

		public override float WorkHour
		{
			get { return ConfigManager.Config.AnalyzeReporter.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			DataTable tradersData = Data.Database.QueryDataTable("SELECT id, name, email FROM traders");
			DataTable tradesData = Data.Database.QueryDataTable("SELECT trader_id, stock_id, SUM(count * action) count FROM trades WHERE DATE(action_time)<=DATE(@time) GROUP BY trader_id, stock_id", "time", CurrentDateTime);
			DataTable analyzeData = Data.Database.QueryDataTable("SELECT s.id stock_id, s.name, s.symbol, a.action, a.worthiness FROM analyzes a INNER JOIN stocks s ON a.stock_id=s.id WHERE DATE(analyze_time)=DATE(@time)", "time", CurrentDateTime);
			analyzeData.DefaultView.Sort = "worthiness DESC";

			analyzeData.DefaultView.RowFilter = "action=1";
			DataTable buyAnalyzeData = analyzeData.DefaultView.ToTable();

			analyzeData.DefaultView.RowFilter = "action=-1";
			DataTable sellAnalyzeData = analyzeData.DefaultView.ToTable();

			StringBuilder buyText = new StringBuilder();
			buyText.AppendLine("Should Buy:");
			for (int i = 0; i < buyAnalyzeData.Rows.Count; ++i)
			{
				DataRow row = buyAnalyzeData.Rows[i];

				buyText.Append(i + 1);
				buyText.Append(". ");
				buyText.Append(row["name"]);
				buyText.Append(" (");
				buyText.Append(row["symbol"]);
				buyText.AppendLine(")");
			}

			for (int i = 0; i < tradersData.Rows.Count; ++i)
			{
				StringBuilder sellText = new StringBuilder();

				DataRow traderRow = tradersData.Rows[i];

				string emailAddress = traderRow["email"].ToString();
				if (string.IsNullOrEmpty(emailAddress))
					continue;

				int sellCount = 0;

				tradesData.DefaultView.RowFilter = "trader_id=" + Convert.ToInt32(traderRow["id"]);
				if (tradesData.DefaultView.Count != 0)
				{
					sellText.AppendLine("Should Sell:");

					for (int j = 0; j < tradesData.DefaultView.Count; ++j)
					{
						DataRowView tradeRow = tradesData.DefaultView[j];

						if (Convert.ToInt32(tradeRow["count"]) <= 0)
							continue;

						sellAnalyzeData.DefaultView.RowFilter = "stock_id=" + tradeRow["stock_id"];
						if (sellAnalyzeData.DefaultView.Count != 0)
						{
							sellText.Append(++sellCount);
							sellText.Append(". ");
							sellText.Append(sellAnalyzeData.DefaultView[0]["name"]);
							sellText.Append(" (");
							sellText.Append(sellAnalyzeData.DefaultView[0]["symbol"]);
							sellText.AppendLine(")");
						}
					}
				}

				SendEmail(traderRow["name"].ToString(), emailAddress, (sellCount == 0 ? "" : sellText.ToString()) + buyText.ToString(), CurrentDateTime);
			}

			return true;
		}

		private static bool SendEmail(string Name, string Email, string Body, DateTime Date)
		{
			MailMessage message = new MailMessage();
			message.From = new MailAddress(ConfigManager.Config.AnalyzeReporter.Username, "Bourse Analyzer");
			message.To.Add(new MailAddress(Email, Name));

			string date = Date.AddDays(1).ToPersianDateTime();

			message.Subject = string.Format(SUBJECT_TEMPLATE, date);
			message.Body = string.Format(BODY_TEMPLATE, date, Body);

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