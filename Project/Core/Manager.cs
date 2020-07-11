using GameFramework.Common.Utilities;
using GameFramework.Common.Web;
using System;
using System.IO;
using System.Threading;

namespace Core
{
	public class Manager
	{
		private class DataUpdater
		{
			private const string END_POINT = "http://members.tsetmc.com/tsev2/excel/MarketWatchPlus.aspx?d=0";
			private const int START_HOUR = 17;

			private DateTime nextUpdateTime = DateTime.MinValue;

			public DataUpdater()
			{
				nextUpdateTime = DateTime.Now.Date;

				if (DateTime.Now.Hour > START_HOUR)
					nextUpdateTime = nextUpdateTime.AddDays(1);

				nextUpdateTime = nextUpdateTime.AddHours(START_HOUR);
			}

			public void Update()
			{
				if (DateTime.Now < nextUpdateTime)
					return;
				nextUpdateTime = nextUpdateTime.AddDays(1);

				ConsoleHelper.WriteInfo("Downloading today's stocks info...");

				byte[] data = null;

				try
				{
					data = Requests.DownloadFile(END_POINT, 1000000);
				}
				catch (Exception e)
				{
					ConsoleHelper.WriteException(e, "Downloading data failed");
				}

				if (data == null)
					return;

				ConsoleHelper.WriteInfo("Importing today's stocks info...");

				XLSXImporter.Import(Data.Database, new XLSXImporter.Info { Time = DateTime.Now, Data = data });

				ConsoleHelper.WriteInfo("Updating today's stocks info done");
			}
		}

		private DataUpdater updater = null;

		public Manager()
		{
			updater = new DataUpdater();
		}

		public void Run()
		{
			while (true)
			{
				updater.Update();

				Thread.Sleep(86400);
			}
		}
	}
}
