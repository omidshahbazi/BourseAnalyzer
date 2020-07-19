using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public class DataUpdater : Worker
	{
		protected override float WorkHour
		{
			get { return ConfigManager.Config.Updater.WorkHour; }
		}

		protected override bool Do()
		{
			ConsoleHelper.WriteInfo("Downloading today's stocks info...");

			DataTable data = DataDownloader.Download();
			if (data == null)
				return false;

			ConsoleHelper.WriteInfo("Importing today's stocks info...");

			if (!XLSXImporter.Import(Data.Database, DateTime.UtcNow, data))
				return false;

			ConsoleHelper.WriteInfo("Updating today's stocks info done");

			return true;
		}
	}
}