using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public class DataUpdater : Worker
	{
		public override bool Enabled
		{
			get { return ConfigManager.Config.DataUpdater.Enabled; }
		}

		public override float WorkHour
		{
			get { return ConfigManager.Config.DataUpdater.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			ConsoleHelper.WriteInfo("Downloading today's stocks info...");

			DataTable data = null;

			//if (CurrentDateTime.Date == DateTime.UtcNow.Date)
			//	data = DataDownloader.DownloadLiveData();
			//else
			data = DataDownloader.Download(CurrentDateTime);

			if (data == null)
				return false;

			ConsoleHelper.WriteInfo("Importing today's stocks info...");

			if (!XLSXImporter.Import(Data.Database, CurrentDateTime, data))
				return false;

			return true;
		}
	}
}