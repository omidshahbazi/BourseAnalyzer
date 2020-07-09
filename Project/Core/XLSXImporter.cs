using GameFramework.DatabaseManaged;
using OfficeOpenXml;
using System;
using System.Data;
using System.IO;

namespace Core
{
	public static class XLSXImporter
	{
		public static void Import(Database Connection, byte[] Data)
		{
			DataTable stocksTable = Connection.ExecuteWithReturnDataTable("SELECT id, symbol, NOW() now_time FROM stocks");

			DateTime time = DateTime.Now;
			if (stocksTable.Rows.Count != 0)
				time = Convert.ToDateTime(stocksTable.Rows[0]["now_time"]);

			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			ExcelPackage package = new ExcelPackage(new MemoryStream(Data));
			ExcelWorksheet sheet = package.Workbook.Worksheets[0];

			const int HEADER_ROW_COUNT = 3;
			int rowCount = sheet.Dimension.Rows - HEADER_ROW_COUNT;
			for (int i = 1; i <= rowCount; ++i)
			{
				int row = HEADER_ROW_COUNT + i;

				string symbol = sheet.GetValue<string>(row, 1, "");
				string name = sheet.GetValue<string>(row, 2, "");

				int count = sheet.GetValue<int>(row, 3, 0);
				int size = sheet.GetValue<int>(row, 4, 0);
				int value = sheet.GetValue<int>(row, 5, 0);
				int yesterday = sheet.GetValue<int>(row, 6, 0);
				int first = sheet.GetValue<int>(row, 7, 0);
				int lastTransactionAmount = sheet.GetValue<int>(row, 8, 0);
				int lastTransactionChange = sheet.GetValue<int>(row, 9, 0);
				float lastTransactionPercent = sheet.GetValue<float>(row, 10, 0);
				int lastPriceAmount = sheet.GetValue<int>(row, 11, 0);
				int lastPriceChange = sheet.GetValue<int>(row, 12, 0);
				float lastPricePercent = sheet.GetValue<float>(row, 13, 0);
				int minimum = sheet.GetValue<int>(row, 14, 0);
				int maximum = sheet.GetValue<int>(row, 15, 0);
				int eps = sheet.GetValue<int>(row, 16, 0);
				float pe = sheet.GetValue<float>(row, 17, 0);
				int buyCount = sheet.GetValue<int>(row, 18, 0);
				int buyAmount = sheet.GetValue<int>(row, 19, 0);
				int buyPrice = sheet.GetValue<int>(row, 20, 0);
				int sellPrice = sheet.GetValue<int>(row, 21, 0);
				int sellAmount = sheet.GetValue<int>(row, 22, 0);
				int sellCount = sheet.GetValue<int>(row, 23, 0);

				int stockID = -1;
				stocksTable.DefaultView.RowFilter = "symbol='" + symbol + "'";
				if (stocksTable.DefaultView.Count == 0)
					stockID = Connection.ExecuteInsert("INSERT INTO stocks(symbol, name) VALUES(@symbol, @name)", "@symbol", symbol, "@name", name);
				else
					stockID = Convert.ToInt32(stocksTable.DefaultView[0]["id"]);

				Connection.ExecuteInsert(
					"INSERT INTO snapshot_data(stock_id, take_time, count, size, value, yesterday, first, last_transaction_amount, last_transaction_change, last_transaction_percent, last_price_amount, last_price_change, last_price_percent, minimum, maximum, eps, pe, buy_count, buy_amount, buy_price, sell_price, sell_amount, sell_count) " +
					"VALUES(@stock_id, @take_time, @count, @size, @value, @yesterday, @first, @last_transaction_amount, @last_transaction_change, @last_transaction_percent, @last_price_amount, @last_price_change, @last_price_percent, @minimum, @maximum, @eps, @pe, @buy_count, @buy_amount, @buy_price, @sell_price, @sell_amount, @sell_count)",
					"@stock_id", stockID,
					"@take_time", time,
					"@count", count,
					"@size", size,
					"@value", value,
					"@yesterday", yesterday,
					"@first", first,
					"@last_transaction_amount", lastTransactionAmount,
					"@last_transaction_change", lastTransactionChange,
					"@last_transaction_percent", (float.IsNaN(lastTransactionPercent) ? 0 : lastTransactionPercent),
					"@last_price_amount", lastPriceAmount,
					"@last_price_change", lastPriceChange,
					"@last_price_percent", (float.IsNaN(lastPricePercent) ? 0 : lastPricePercent),
					"@minimum", minimum,
					"@maximum", maximum,
					"@eps", eps,
					"@pe", (float.IsNaN(pe) ? 0 : pe),
					"@buy_count", buyCount,
					"@buy_amount", buyAmount,
					"@buy_price", buyPrice,
					"@sell_price", sellPrice,
					"@sell_amount", sellAmount,
					"@sell_count", sellCount);
			}
		}
	}
}
