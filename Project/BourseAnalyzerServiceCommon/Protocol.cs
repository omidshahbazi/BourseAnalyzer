namespace BourseAnalyzerServiceCommon
{
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
		public string[] StocksSymbol;
	}
}
