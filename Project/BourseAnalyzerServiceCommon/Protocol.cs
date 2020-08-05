namespace BourseAnalyzerServiceCommon
{
	public class StockInfo
	{
		public int ID;
		public string Symbol;
	}

	public class TradeInfo
	{
		public int ID;
		public string Symbol;
		public int Price;
		public int Count;
		public int TotalPrice;
		public int Action;
		public double Time;
	}

	public class LoginReq
	{
		public string Username;
		public string Password;
	}

	public class LoginRes
	{
		public bool Result;
		public string Message;
		public int TraderID;
	}

	public class GetBasicDataReq
	{
		public int TraderID;
	}

	public class GetBasicDataRes
	{
		public StockInfo[] Stocks;
	}

	public class GetTradeDataReq
	{
		public int TraderID;
	}

	public class GetTradeDataRes
	{
		public TradeInfo[] AllTrades;
		public TradeInfo[] TotalTrades;
	}

	public class DeleteTradeReq
	{
		public int TradeID;
	}

	public class DeleteTradeRes
	{
	}

	public class AddTradeReq
	{
		public int TraderID;
		public int StockID;
		public int Price;
		public int Count;
		public int Action;
		public double Time;
	}

	public class AddTradeRes
	{
	}
}
