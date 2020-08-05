using BourseAnalyzerServiceCommon;
using System.Windows.Forms;

namespace BourseAnalyzerClient
{
	public partial class LoginForm : Form
	{
		private object lockObject = new object();

		public LoginForm()
		{
			InitializeComponent();

			UsernameTextBox.Text = ConfigManager.Config.LastUsername;

			Timer serviceTimer = new Timer();
			serviceTimer.Interval = 100;
			serviceTimer.Tick += ServiceTimer_Tick;
			serviceTimer.Start();
		}

		private void ServiceTimer_Tick(object sender, System.EventArgs e)
		{
			Networking.Service();
		}

		private void LoginButton_Click(object sender, System.EventArgs e)
		{
			ConfigManager.Config.LastUsername = UsernameTextBox.Text;
			ConfigManager.Save();

			Networking.Connection.Send<LoginReq, LoginRes>(new LoginReq() { Username = UsernameTextBox.Text, Password = PasswordTextBox.Text }, (res) =>
			{
				if (res.Result)
				{
					Program.TraderID = res.TraderID;
					Program.State = Program.States.Client;

					lock (lockObject)
					{
						Close();
					}
				}
				else
					MessageBox.Show(res.Message);
			});
		}
	}
}