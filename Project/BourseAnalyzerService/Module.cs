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
			DataTable tradersData = database.QueryDataTable("SELECT id, symbol FROM stocks");

			GetBasicDataRes res = new GetBasicDataRes();

			res.StocksSymbol = new string[tradersData.Rows.Count];
			for (int i = 0; i < tradersData.Rows.Count; ++i)
			{
				DataRow row = tradersData.Rows[i];

				res.StocksSymbol[i] = row["symbol"].ToString();
			}

			return res;
		}
	}
}