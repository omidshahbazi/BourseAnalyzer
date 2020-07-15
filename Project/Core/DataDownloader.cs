using GameFramework.Common.Utilities;
using GameFramework.Common.Web;
using System;
using System.Data;

namespace Core
{
	public static class DataDownloader
	{
		private const string END_POINT = "http://members.tsetmc.com/tsev2/excel/MarketWatchPlus.aspx?d=0";

		public static DataTable Download()
		{
			byte[] data = null;

			try
			{
				data = Requests.DownloadFile(END_POINT, 1000000);
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