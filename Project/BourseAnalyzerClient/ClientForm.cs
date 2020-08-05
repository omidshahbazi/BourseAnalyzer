using BourseAnalyzerServiceCommon;
using System;
using System.Data;
using System.Windows.Forms;

namespace BourseAnalyzerClient
{
	partial class ClientForm : BaseForm
	{
		public ClientForm()
		{
			InitializeComponent();

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

			Networking.Connection.OnDisconnected += Connection_OnDisconnected;

			Program.State = Program.States.Close;
		}

		private void Connection_OnDisconnected(Backend.Common.NetworkSystem.Connection Connection)
		{
			Program.State = Program.States.Connecting;
			Close();
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

			if (StocksComboBox.Items.Count != 0)
				StocksComboBox.SelectedIndex = 0;
		}

		private void TotalCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateTradeDataGridView();
		}

		private void DeleteButton_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Whould you like to delete the entry?", "Delete entry", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
				return;

			DataGridViewRow dgvr = TradesDataGridView.SelectedRows[0];
			DataRowView row = (DataRowView)dgvr.DataBoundItem;

			Networking.Connection.Send<DeleteTradeReq, DeleteTradeRes>(new DeleteTradeReq() { TradeID = Convert.ToInt32(row["id"]) }, (res) =>
			{
				if (res != null)
					UpdateTradeData();
			});
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			DataRowView row = (DataRowView)StocksComboBox.SelectedItem;

			Networking.Connection.Send<AddTradeReq, AddTradeRes>(new AddTradeReq() { TraderID = Data.TraderID, StockID = Convert.ToInt32(row["id"]), Price = (int)PriceNumericUpDown.Value, Count = (int)CountNumericUpDown.Value, Action = (BuyRadioButton.Checked ? 1 : -1), Time = (ActionDateTimePicker.Value - new DateTime(1970, 1, 1)).TotalSeconds }, (res) =>
			{
				if (res != null)
					UpdateTradeData();
			});
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
			TradesDataGridView.DataSource = (TotalCheckBox.Checked ? Data.TotalTrades : Data.AllTrades);

			DeleteButton.Enabled = (TradesDataGridView.RowCount != 0 && !TotalCheckBox.Checked);
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
					tradeInfo.TotalPrice,
					(tradeInfo.Action > 0 ? "Buy" : "Sell"),
					startTime.AddSeconds(tradeInfo.Time)
					);
			}

			return data;
		}
	}
}