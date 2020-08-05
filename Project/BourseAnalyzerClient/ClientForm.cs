using BourseAnalyzerServiceCommon;
using System;
using System.Data;
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

			TradesDataGridView.AutoGenerateColumns = false;
			ActionDateTimePicker.Value = DateTime.Now;

			Networking.Connection.Send<GetBasicDataReq, GetBasicDataRes>(new GetBasicDataReq() { TraderID = Data.TraderID }, (res) =>
			{
				Data.Stocks = new DataTable();

				Data.Stocks.Columns.Add("id");
				Data.Stocks.Columns.Add("symbol");

				for (int i = 0; i < res.Stocks.Length; ++i)
				{
					StockInfo stockInfo = res.Stocks[i];

					Data.Stocks.Rows.Add(stockInfo.ID, stockInfo.Symbol);
				}
			});

			UpdateTradeData();
		}

		private void ServiceTimer_Tick(object sender, EventArgs e)
		{
			Networking.Service();
		}

		private void TradesDataGridView_SelectionChanged(object sender, EventArgs e)
		{
			if (TradesDataGridView.SelectedRows.Count == 0)
				return;

			DataGridViewRow dgvr = TradesDataGridView.SelectedRows[0];

			StockFilterTextBox.Text = dgvr.Cells["SymbolColumn"].Value.ToString();
		}

		private void StockFilterTextBox_TextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(StockFilterTextBox.Text))
			{
				StocksComboBox.DataSource = null;
				return;
			}

			Data.Stocks.DefaultView.RowFilter = string.Format("symbol LIKE '{0}%'", StockFilterTextBox.Text);
			StocksComboBox.DataSource = Data.Stocks.DefaultView.ToTable();
			StocksComboBox.ValueMember = "id";
			StocksComboBox.DisplayMember = "symbol";
			StocksComboBox.BindingContext = BindingContext;
		}

		private void TotalCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateTradeDataGridView();

		}

		private void UpdateTradeData()
		{
			Networking.Connection.Send<GetTradeDataReq, GetTradeDataRes>(new GetTradeDataReq() { TraderID = Data.TraderID }, (res) =>
			{
				Data.AllTrades = GenerateTradeData(res.AllTrades);
				Data.TotalTrades = GenerateTradeData(res.TotalTrades);

				UpdateTradeDataGridView();
			});
		}

		private void UpdateTradeDataGridView()
		{
			DeleteButton.Enabled = !TotalCheckBox.Checked;

			TradesDataGridView.DataSource = (TotalCheckBox.Checked ? Data.TotalTrades : Data.AllTrades);
		}

		private DataTable GenerateTradeData(TradeInfo[] Trades)
		{
			DataTable data = new DataTable();

			data.Columns.Add("id", typeof(int));
			data.Columns.Add("symbol", typeof(string));
			data.Columns.Add("price", typeof(int));
			data.Columns.Add("count", typeof(int));
			data.Columns.Add("total_price", typeof(int));
			data.Columns.Add("action", typeof(string));
			data.Columns.Add("time", typeof(DateTime));

			DateTime startTime = new DateTime(1970, 1, 1);

			for (int i = 0; i < Trades.Length; ++i)
			{
				TradeInfo tradeInfo = Trades[i];

				data.Rows.Add(
					tradeInfo.ID,
					tradeInfo.Symbol,
					tradeInfo.Price,
					tradeInfo.Count,
					tradeInfo.Price * tradeInfo.Count,
					(tradeInfo.Action > 0 ? "Buy" : "Sell"),
					startTime.AddSeconds(tradeInfo.Time)
					);
			}

			return data;
		}
	}
}