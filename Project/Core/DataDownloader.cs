﻿using GameFramework.Common.Utilities;
using GameFramework.Common.Web;
using System;
using System.Data;
using System.Globalization;

namespace Core
{
	public static class DataDownloader
	{
		private const string END_POINT = "http://members.tsetmc.com/tsev2/excel/MarketWatchPlus.aspx?d=";
		private const string LIVE_END_POINT = END_POINT + "0";

		public static DataTable DownloadLiveData()
		{
			byte[] data = null;

			try
			{
				data = Requests.DownloadFile(LIVE_END_POINT, 1000000);
			}
			catch (Exception e)
			{
				ConsoleHelper.WriteException(e, "Downloading data failed");

				return null;
			}

			return XLSXConverter.ToDataTable(data);
		}

		public static DataTable Download(DateTime Date)
		{
			PersianCalendar persianCalendar = new PersianCalendar();

			string date = string.Format("{0}/{1}/{2}", persianCalendar.GetYear(Date), persianCalendar.GetMonth(Date), persianCalendar.GetDayOfMonth(Date));

			byte[] data = null;

			try
			{
				data = Requests.DownloadFile(END_POINT + date, 1000000);
			}
			catch (Exception e)
			{
				ConsoleHelper.WriteException(e, "Downloading data failed");

				return null;
			}

			return XLSXConverter.ToDataTable(data);
		}
	}
}