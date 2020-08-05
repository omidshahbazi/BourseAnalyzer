using System;
using System.Windows.Forms;

namespace BourseAnalyzerClient
{
	public partial class BaseForm : Form
	{
		private Timer serviceTimer = null;

		public BaseForm()
		{
			serviceTimer = new Timer();
			serviceTimer.Interval = 500;
			serviceTimer.Tick += ServiceTimer_Tick;
			serviceTimer.Start();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			serviceTimer.Stop();
		}

		private void ServiceTimer_Tick(object sender, EventArgs e)
		{
			Networking.Service();
		}
	}
}
