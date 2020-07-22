using System;
using System.Data;

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
			public DataTable LiveData;
		}

		public class Result
		{
			public int Action;
			public double Worthiness;
		}
	}
}