using Backend.Client;

namespace BourseAnalyzerClient
{
	static class Networking
	{
		public static ServerConnection Connection
		{
			get;
			private set;
		}

		static Networking()
		{
			Connection = new ServerConnection();
			Connection.ReceiveBufferSize = Connection.SendBufferSize = 1204 * 1024 * 10;

			Connection.Connect(Backend.Common.ProtocolTypes.TCP, ConfigManager.Config.Host, ConfigManager.Config.Port);
		}

		public static void Service()
		{
			Connection.Service();
		}
	}
}
