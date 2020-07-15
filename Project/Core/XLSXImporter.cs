using GameFramework.DatabaseManaged;
using System;
using System.Data;
using System.Text;

namespace Core
{
	public static class XLSXImporter
	{
		public static bool Import(Database Connection, DateTime Time, DataTable Data)
		{
			DataTable stocksTable = Connection.QueryDataTable("SELECT id, symbol FROM stocks");

			if (Data.Rows.Count == 0)
				return true;

			for (int i = 0; i < Data.Rows.Count; ++i)
			{
				DataRow row = Data.Rows[i];

				string symbol = row["symbol"].ToString();
				string name = row["name"].ToString();

				stocksTable.DefaultView.RowFilter = "symbol='" + symbol + "'";
				if (stocksTable.DefaultView.Count != 0)
					continue;

				int id = Connection.ExecuteInsert("INSERT INTO stocks(symbol, name) VALUES(@symbol, @name)", "symbol", symbol, "name", name);

				stocksTable.Rows.Add(id, symbol);
			}

			StringBuilder builder = new StringBuilder();

			string dateTime = Time.ToString("yyyy/MM/dd hh:mm:ss");

			for (int i = 0; i < Data.Rows.Count; ++i)
			{
				DataRow row = Data.Rows[i];

				string symbol = row["symbol"].ToString();

				int count = Convert.ToInt32(row["count"]);
				long volume = Convert.ToInt64(row["volume"]);
				long value = Convert.ToInt64(row["value"]);
				int open = Convert.ToInt32(row["open"]);
				int first = Convert.ToInt32(row["first"]);
				int high = Convert.ToInt32(row["high"]);
				int low = Convert.ToInt32(row["low"]);
				int last = Convert.ToInt32(row["last"]);
				int close = Convert.ToInt32(row["close"]);

				stocksTable.DefaultView.RowFilter = "symbol='" + symbol + "'";
				int stockID = Convert.ToInt32(stocksTable.DefaultView[0]["id"]);

				builder.Append("INSERT INTO snapshots(stock_id, take_time, count, volume, value, open, first, high, low, last, close) VALUES(");
				builder.Append(stockID);
				builder.Append(",'");
				builder.Append(dateTime);
				builder.Append("',");
				builder.Append(count);
				builder.Append(',');
				builder.Append(volume);
				builder.Append(',');
				builder.Append(value);
				builder.Append(',');
				builder.Append(open);
				builder.Append(',');
				builder.Append(first);
				builder.Append(',');
				builder.Append(high);
				builder.Append(',');
				builder.Append(low);
				builder.Append(',');
				builder.Append(last);
				builder.Append(',');
				builder.Append(close);
				builder.Append(");");
			}

			Connection.Execute(builder.ToString());

			return true;
		}
	}
}