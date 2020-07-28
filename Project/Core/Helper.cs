using GameFramework.ASCIISerializer;
using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Core
{
	static class Helper
	{
		public static double CalculateChanges(DataTable SnapshotsData, int StockID, DateTime PreviousDate, DateTime CurrentDate)
		{
			SnapshotsData.DefaultView.RowFilter = string.Format("stock_id={0} AND take_time='{1}'", StockID, PreviousDate.Date.ToDatabaseDateTime());
			if (SnapshotsData.DefaultView.Count != 0)
			{
				int prevClose = Convert.ToInt32(SnapshotsData.DefaultView[0]["close"]);

				SnapshotsData.DefaultView.RowFilter = string.Format("stock_id={0} AND take_time='{1}'", StockID, CurrentDate.Date.ToDatabaseDateTime());
				if (SnapshotsData.DefaultView.Count != 0)
				{
					int currClose = Convert.ToInt32(SnapshotsData.DefaultView[0]["close"]);

					return (currClose / (double)prevClose) - 1;
				}
			}

			return double.NaN;
		}

		public static bool SendEmail(string Subject, string HTMLBody, ISerializeArray EmailsArray, string Name)
		{
			MailMessage message = new MailMessage();
			message.From = new MailAddress(ConfigManager.Config.AnalyzeReporter.Username, "Bourse Analyzer");

			message.To.Add(new MailAddress(EmailsArray.Get<string>(0), Name));
			for (uint i = 1; i < EmailsArray.Count; ++i)
				message.CC.Add(new MailAddress(EmailsArray.Get<string>(i), Name));

			message.Subject = Subject;
			message.Body = HTMLBody;

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

		public static void HTML_WriteHeader2(StringBuilder Builder, string Content, Color Color)
		{
			HTMLGenerator.Style.Color = Color;
			HTMLGenerator.BeginHeader2(Builder);
			HTMLGenerator.Style.Color = Color.Black;
			HTMLGenerator.WriteContent(Builder, Content);
			HTMLGenerator.EndHeader2(Builder);
		}

		public static void WriteToFile(string Dir, DateTime Time, string FileName, string Content)
		{
			string path = Path.GetFullPath(Dir);

			path = Path.Combine(path, Time.ToString("yyyy-MM-dd"));

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = Path.Combine(path, FileName);

			File.WriteAllText(path, Content);
		}
	}
}