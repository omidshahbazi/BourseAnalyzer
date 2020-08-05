using BourseAnalyzerServiceCommon;
using System.Windows.Forms;

namespace BourseAnalyzerClient
{
	partial class ClientForm : Form
	{
		public ClientForm()
		{
			InitializeComponent();

			Timer serviceTimer = new Timer();
			serviceTimer.Interval = 100;
			serviceTimer.Tick += ServiceTimer_Tick;
			serviceTimer.Start();

			Networking.Connection.Send<GetBasicDataReq, GetBasicDataRes>(new GetBasicDataReq() { TraderID = Program.TraderID }, (res) =>
			{

			});
		}

		private void ServiceTimer_Tick(object sender, System.EventArgs e)
		{
			Networking.Service();
		}
	}
}