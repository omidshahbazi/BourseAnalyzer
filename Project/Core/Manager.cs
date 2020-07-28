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
					DateTime nowTime = Convert.ToDateTime(workerDateRow["now"]);

					for (int j = 0; j < workers.Length; ++j)
					{
						Worker worker = workers[j];

						string workerName = worker.GetType().Name;

						DataTable workerSchedulesData = Data.QueryDataTable("SELECT id, schedule_time FROM worker_schedules WHERE DATE(schedule_time)=DATE(@date) AND name=@name AND done=0", "date", date, "name", workerName);
						for (int k = 0; k < workerSchedulesData.Rows.Count; ++k)
						{
							DataRow row = workerSchedulesData.Rows[k];

							DateTime scheduleTime = Convert.ToDateTime(row["schedule_time"]);

							if (scheduleTime > nowTime)
								continue;

							if (worker.Enabled)
							{
								ConsoleHelper.WriteInfo("{0} is working on {1}", workerName, scheduleTime);

								if (!worker.Do(scheduleTime))
									break;

								ConsoleHelper.WriteInfo("{0} done", workerName);
							}

							query.Append("UPDATE worker_schedules SET done=1 WHERE id=");
							query.Append(row["id"]);
							query.Append(';');
						}

						DateTime nextScheduleTime = nowTime.Date.AddDays(1);
						if (nextScheduleTime.DayOfWeek == DayOfWeek.Thursday)
							nextScheduleTime = nextScheduleTime.Date.AddDays(1);
						if (nextScheduleTime.DayOfWeek == DayOfWeek.Friday)
							nextScheduleTime = nextScheduleTime.Date.AddDays(1);

						workerSchedulesData.DefaultView.RowFilter = string.Format("'{0}'<=schedule_time AND schedule_time<='{1}'", nextScheduleTime.ToDatabaseDateTime(), nextScheduleTime.AddDays(1).ToDatabaseDateTime());
						if (workerSchedulesData.DefaultView.Count == 0)
						{
							query.Append("INSERT INTO worker_schedules(name, schedule_time, done) VALUES(");
							query.Append('\'');
							query.Append(workerName);
							query.Append("\','");
							query.Append(nextScheduleTime.AddHours(worker.WorkHour).ToDatabaseDateTime());
							query.Append("\',0);");
						}

						if (query.Length != 0)
						{
							Data.Execute(query.ToString());

							query = new StringBuilder();
						}
					}
				}

				Thread.Sleep(ConfigManager.Config.CheckSchedulesPeriod * 1000);
			}
		}
	}
}