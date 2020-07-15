using OfficeOpenXml;
using System.Data;
using System.IO;

namespace Core
{
	public static class XLSXConverter
	{
		public static DataTable ToDataTable(byte[] Data)
		{
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			ExcelPackage package = new ExcelPackage(new MemoryStream(Data));
			ExcelWorksheet sheet = package.Workbook.Worksheets[0];

			const int HEADER_ROW_COUNT = 3;
			int rowCount = sheet.Dimension.Rows - HEADER_ROW_COUNT;

			DataTable table = new DataTable();
			table.Columns.Add("symbol", typeof(string));
			table.Columns.Add("name", typeof(string));
			table.Columns.Add("count", typeof(int));
			table.Columns.Add("volume", typeof(long));
			table.Columns.Add("value", typeof(long));
			table.Columns.Add("open", typeof(int));
			table.Columns.Add("first", typeof(int));
			table.Columns.Add("high", typeof(int));
			table.Columns.Add("low", typeof(int));
			table.Columns.Add("last", typeof(int));
			table.Columns.Add("close", typeof(int));

			if (rowCount == 0)
				return table;

			for (int i = 1; i <= rowCount; ++i)
			{
				int row = HEADER_ROW_COUNT + i;

				string symbol = sheet.GetValue<string>(row, 1, "");
				string name = sheet.GetValue<string>(row, 2, "");
				int count = sheet.GetValue<int>(row, 3, 0);
				long volume = sheet.GetValue<long>(row, 4, 0);
				long value = sheet.GetValue<long>(row, 5, 0);
				int open = sheet.GetValue<int>(row, 6, 0);
				int first = sheet.GetValue<int>(row, 7, 0);
				int high = sheet.GetValue<int>(row, 15, 0);
				int low = sheet.GetValue<int>(row, 14, 0);
				int last = sheet.GetValue<int>(row, 8, 0);
				int close = sheet.GetValue<int>(row, 11, 0);

				table.Rows.Add(symbol, name, count, volume, value, open, first, high, low, last, close);
			}

			return table;
		}
	}
}