using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Text;
using System.Threading;

namespace Core
{
	public class Manager
	{
		private static readonly Worker[] workers = new Worker[] { new DataUpdater(), new AnalyzeValidator(), new DataAnalyzer() };

		public void Run()
		{
			while (true)
			{
				DateTime nowTime = DateTime.UtcNow;

				StringBuilder query = new StringBuilder();

				for (int i = 0; i < workers.Length; ++i)
				{
					Worker worker = workers[i];

					string workerName = worker.GetType().Name;

					DataTable workerSchedulesData = Data.Database.QueryDataTable("SELECT id, schedule_time, UTC_TIMESTAMP() now FROM worker_schedules WHERE name=@name AND done=0", "name", workerName);
					for (int j = 0; j < workerSchedulesData.Rows.Count; ++j)
					{
						DataRow row = workerSchedulesData.Rows[j];

						DateTime scheduleTime = Convert.ToDateTime(row["schedule_time"]);
						nowTime = Convert.ToDateTime(row["now"]);

						if (scheduleTime > nowTime)
							continue;

						ConsoleHelper.WriteInfo("{0} is working on {1}", workerName, scheduleTime);

						if (!workers[i].Do(scheduleTime))
							break;

						ConsoleHelper.WriteInfo("{0} done", workerName);

						query.Append("UPDATE worker_schedules SET done=1 WHERE id=");
						query.Append(row["id"]);
						query.Append(';');
					}

					DateTime nextScheduleTime = nowTime.Date.AddDays(1);

					workerSchedulesData.DefaultView.RowFilter = "'" + nextScheduleTime.ToDatabaseDateTime() + "'<=schedule_time AND schedule_time<='" + nextScheduleTime.AddDays(1).ToDatabaseDateTime() + "'";
					if (workerSchedulesData.DefaultView.Count == 0)
					{
						query.Append("INSERT INTO worker_schedules(name, schedule_time, done) VALUES(");
						query.Append('\'');
						query.Append(workerName);
						query.Append("\','");
						query.Append(nextScheduleTime.AddHours(worker.WorkHour).ToDatabaseDateTime());
						query.Append("\',0);");
					}
				}

				if (query.Length != 0)
					Data.Database.Execute(query.ToString());

				Thread.Sleep(ConfigManager.Config.CheckSchedulesPeriod * 1000);
			}
		}
	}
}