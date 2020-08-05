using System;
using System.Windows.Forms;

namespace BourseAnalyzerClient
{
	static class Program
	{
		public enum States
		{
			Connecting,
			Close,
			Login,
			Client
		}

		public static States State = States.Connecting;

		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Networking.Connection.OnConnected += Connection_OnConnected;
			Networking.Connection.OnConnectionFailed += Connection_OnConnectionFailed;

			Networking.Connect();

			while (true)
			{
				while (State == States.Connecting)
					Networking.Service();

				if (State == States.Login)
				{
					LoginForm loginForm = new LoginForm();
					loginForm.ShowDialog();

					if (State != States.Client)
						State = States.Close;

					continue;
				}

				if (State == States.Client)
				{
					ClientForm clientForm = new ClientForm();
					clientForm.ShowDialog();

					if (State != States.Close)
						Networking.Connect();

					continue;
				}

				break;
			}
		}

		private static void Connection_OnConnected(Backend.Common.NetworkSystem.Connection Connection)
		{
			State = States.Login;
		}

		private static void Connection_OnConnectionFailed(Backend.Common.NetworkSystem.Connection Connection)
		{
			State = States.Close;
		}
	}
}
