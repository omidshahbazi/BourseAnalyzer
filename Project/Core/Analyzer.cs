using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace Core
{
	public static partial class Analyzer
	{
		public class Info
		{
			public DateTime DateTime;

			public int ID;

			public string Symbol;

			public DataTable HistoryData;
		}

		public class Result
		{
			public int Action;
			public double Worthiness;
		}

		private static void WriteCSV(string Dir, Info Info, DataTable Data)
		{
			string path = Path.GetFullPath(Dir);
			path = Path.Combine(path, Info.DateTime.ToPersianDateTime().Replace('/', '-'));

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = Path.Combine(path, Info.ID + "_" + Info.Symbol + ".csv");

			StringBuilder builder = new StringBuilder();
			CSVWriter.Write(builder, 0, 0, Data);

			File.WriteAllText(path, builder.ToString());
		}
	}
}