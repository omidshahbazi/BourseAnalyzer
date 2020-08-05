using BourseAnalyzerServiceCommon;
using System.Windows.Forms;

namespace BourseAnalyzerClient
{
	public partial class LoginForm : BaseForm
	{
		public LoginForm()
		{
			InitializeComponent();

			UsernameTextBox.Text = ConfigManager.Config.LastUsername;
		}

		private void LoginButton_Click(object sender, System.EventArgs e)
		{
			ConfigManager.Config.LastUsername = UsernameTextBox.Text;
			ConfigManager.Save();

			Networking.Connection.Send<LoginReq, LoginRes>(new LoginReq() { Username = UsernameTextBox.Text, Password = PasswordTextBox.Text }, (res) =>
			{
				if (res.Result)
				{
					Data.TraderID = res.TraderID;
					Program.State = Program.States.Client;

						Close();
				}
				else
					MessageBox.Show(res.Message, "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			});
		}
	}
}