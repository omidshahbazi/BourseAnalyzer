using Backend.Base;
using Backend.Base.ModuleSystem;
using Backend.Base.NetworkSystem;
using BourseAnalyzerServiceCommon;
using GameFramework.DatabaseManaged;
using System;
using System.Data;

namespace BourseAnalyzerService
{
	class Module : IModule
	{
		private Database database = null;

		public void Initialize(IContext Context, object Config)
		{
			Config config = (Config)Config;

			database = new MySQLDatabase(config.DatabaseConnection);

			Context.RequestManager.RegisterHandler<LoginReq, LoginRes>(LoginHandler);
			Context.RequestManager.RegisterHandler<GetBasicDataReq, GetBasicDataRes>(GetBasicDataHandler);
			Context.RequestManager.RegisterHandler<GetTradeDataReq, GetTradeDataRes>(GetTradeDataHandler);
			Context.RequestManager.RegisterHandler<DeleteTradeReq, DeleteTradeRes>(DeleteTradeHandler);
			Context.RequestManager.RegisterHandler<AddTradeReq, AddTradeRes>(AddTradeHandler);
		}

		public void Service()
		{
		}

		public void Shutdown()
		{
		}

		private LoginRes LoginHandler(IClient Client, LoginReq Req)
		{
			DataTable tradersData = database.QueryDataTable("SELECT id, password FROM traders WHERE username=@username LIMIT 1", "username", Req.Username);

			if (tradersData.Rows.Count == 0)
				return new LoginRes() { Result = false, Message = "Invlid username", TraderID = -1 };

			DataRow row = tradersData.Rows[0];
			if (row["password"].ToString() == Req.Password)
				return new LoginRes() { Result = true, Message = "", TraderID = Convert.ToInt32(row["id"]) };

			return new LoginRes() { Result = false, Message = "Invalid password", TraderID = -1 };
		}

		private GetBasicDataRes GetBasicDataHandler(IClient Client, GetBasicDataReq Req)
		{
			DataTable stocksData = database.QueryDataTable("SELECT id, symbol FROM stocks");

			GetBasicDataRes res = new GetBasicDataRes();

			res.Stocks = new StockInfo[stocksData.Rows.Count];
			for (int i = 0; i < stocksData.Rows.Count; ++i)
			{
				DataRow row = stocksData.Rows[i];

				res.Stocks[i] = new StockInfo() { ID = Convert.ToInt32(row["id"]), Symbol = row["symbol"].ToString() };
			}

			return res;
		}

		private GetTradeDataRes GetTradeDataHandler(IClient Client, GetTradeDataReq Req)
		{
			GetTradeDataRes res = new GetTradeDataRes();

			res.AllTrades = GenerateTradeData(database.QueryDataTable("SELECT t.id, s.symbol, t.price, t.count, t.price*t.count total_price, t.action, UNIX_TIMESTAMP(t.action_time) action_time FROM trades t INNER JOIN stocks s ON t.stock_id=s.id WHERE trader_id=@trader_id ORDER BY t.action_time", "trader_id", Req.TraderID));
			res.TotalTrades = GenerateTradeData(database.QueryDataTable("SELECT t.id, s.symbol, AVG(t.price) price, SUM(t.count*t.action) count, SUM(t.price*t.count*t.action) total_price, SUM(t.action) action, UNIX_TIMESTAMP(MAX(t.action_time)) action_time FROM trades t INNER JOIN stocks s ON t.stock_id=s.id WHERE trader_id=@trader_id GROUP BY t.stock_id ORDER BY t.action_time", "trader_id", Req.TraderID));

			return res;
		}

		private DeleteTradeRes DeleteTradeHandler(IClient Client, DeleteTradeReq Req)
		{
			database.Execute("DELETE FROM trades WHERE id=@id", "id", Req.TradeID);

			return new DeleteTradeRes();
		}

		private AddTradeRes AddTradeHandler(IClient Client, AddTradeReq Req)
		{
			database.Execute("Insert INTO trades(trader_id, stock_id, price, count, action, action_time) VALUES(@trader_id, @stock_id, @price, @count, @action, FROM_UNIXTIME(@action_time))",
				"trader_id", Req.TraderID,
				"stock_id", Req.StockID,
				"price", Req.Price,
				"count", Req.Count,
				"action", Req.Action,
				"action_time", Req.Time); 

			return new AddTradeRes();
		}

		private TradeInfo[] GenerateTradeData(DataTable Data)
		{
			TradeInfo[] info = new TradeInfo[Data.Rows.Count];

			for (int i = 0; i < Data.Rows.Count; ++i)
			{
				DataRow row = Data.Rows[i];

				info[i] = new TradeInfo()
				{
					ID = Convert.ToInt32(row["id"]),
					Symbol = row["symbol"].ToString(),
					Price = Convert.ToInt32(row["price"]),
					Count = Convert.ToInt32(row["count"]),
					TotalPrice = Convert.ToInt32(row["total_price"]),
					Action = Convert.ToInt32(row["action"]),
					Time = Convert.ToDouble(row["action_time"])
				};
			}

			return info;
		}
	}
}