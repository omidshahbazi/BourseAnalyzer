using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Text;
using System.Threading;

namespace Core
{
	public class Manager
	{
		private static readonly Worker[] workers = new Worker[] { new DataUpdater(), new AnalyzeValidator(), new DataAnalyzer(), new AnalyzeReporter(), new ValidationReporter() };

		public void Run()
		{
			while (true)
			{
				StringBuilder query = new StringBuilder();

				DataTable workerDatesData = Data.QueryDataTable("SELECT DATE(schedule_time) schedule_date, UTC_TIMESTAMP() now FROM worker_schedules where done=0 GROUP BY DATE(schedule_time) ORDER BY schedule_time");

				for (int i = 0; i < workerDatesData.Rows.Count; ++i)
				{
					DataRow workerDateRow = workerDatesData.Rows[i];

					DateTime date = Convert.ToDateTime(workerDateRow["schedule_date"]);

					DataTable workerSchedulesData = Data.QueryDataTable("SELECT id, name, schedule_time FROM worker_schedules WHERE DATE(schedule_time)=DATE(@date) AND schedule_time<UTC_TIMESTAMP() AND done=0", "date", date);
					for (int j = 0; j < workerSchedulesData.Rows.Count; ++j)
					{
						DataRow workerSchedulesRow = workerSchedulesData.Rows[j];

						string workerName = workerSchedulesRow["name"].ToString();
						Worker worker = FindWorker(workerName);
						if (worker == null)
						{
							ConsoleHelper.WriteError("Couldn't find worker {0}", workerName);
							continue;
						}

						DateTime scheduleTime = Convert.ToDateTime(workerSchedulesRow["schedule_time"]);

						if (worker.Enabled)
						{
							ConsoleHelper.WriteInfo("{0} is working on {1}", workerName, scheduleTime);

							if (!worker.Do(scheduleTime))
								break;

							ConsoleHelper.WriteInfo("{0} done", workerName);
						}

						query.Append("UPDATE worker_schedules SET done=1 WHERE id=");
						query.Append(workerSchedulesRow["id"]);
						query.Append(';');
					}
				}


				DateTime nowTime = (workerDatesData.Rows.Count == 0 ? DateTime.UtcNow : Convert.ToDateTime(workerDatesData.Rows[workerDatesData.Rows.Count - 1]["now"]));

				DateTime nextScheduleTime = nowTime.Date.AddDays(1);
				if (nextScheduleTime.DayOfWeek == DayOfWeek.Thursday)
					nextScheduleTime = nextScheduleTime.Date.AddDays(1);
				if (nextScheduleTime.DayOfWeek == DayOfWeek.Friday)
					nextScheduleTime = nextScheduleTime.Date.AddDays(1);

				for (int i = 0; i < workers.Length; ++i)
				{
					Worker worker = workers[i];

					string workerName = worker.GetType().Name;

					DataTable workerSchedulesData = Data.QueryDataTable("SELECT id, name, schedule_time FROM worker_schedules WHERE DATE(schedule_time)=DATE(@date)", "date", nextScheduleTime);
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
					Data.Execute(query.ToString());

				Thread.Sleep(ConfigManager.Config.CheckSchedulesPeriod * 1000);
			}
		}

		private Worker FindWorker(string Name)
		{
			for (int i = 0; i < workers.Length; ++i)
			{
				Worker worker = workers[i];

				if (worker.GetType().Name != Name)
					continue;

				return worker;
			}

			return null;
		}
	}
}