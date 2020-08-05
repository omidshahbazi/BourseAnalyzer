namespace BourseAnalyzerClient
{
	partial class ClientForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.StocksComboBox = new System.Windows.Forms.ComboBox();
			this.PriceNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.CountNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.BuyRadioButton = new System.Windows.Forms.RadioButton();
			this.SellRadioButton = new System.Windows.Forms.RadioButton();
			this.ActionDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.TradesDataGridView = new System.Windows.Forms.DataGridView();
			this.label6 = new System.Windows.Forms.Label();
			this.StockFilterTextBox = new System.Windows.Forms.TextBox();
			this.SymbolColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.TotalPriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ActionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.TimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.TotalCheckBox = new System.Windows.Forms.CheckBox();
			this.DeleteButton = new System.Windows.Forms.Button();
			this.SaveButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.PriceNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CountNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TradesDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// StocksComboBox
			// 
			this.StocksComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.StocksComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.StocksComboBox.FormattingEnabled = true;
			this.StocksComboBox.Location = new System.Drawing.Point(705, 41);
			this.StocksComboBox.Name = "StocksComboBox";
			this.StocksComboBox.Size = new System.Drawing.Size(200, 22);
			this.StocksComboBox.Sorted = true;
			this.StocksComboBox.TabIndex = 0;
			// 
			// PriceNumericUpDown
			// 
			this.PriceNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PriceNumericUpDown.Location = new System.Drawing.Point(705, 70);
			this.PriceNumericUpDown.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
			this.PriceNumericUpDown.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.PriceNumericUpDown.Name = "PriceNumericUpDown";
			this.PriceNumericUpDown.Size = new System.Drawing.Size(200, 22);
			this.PriceNumericUpDown.TabIndex = 2;
			this.PriceNumericUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(665, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(36, 14);
			this.label2.TabIndex = 3;
			this.label2.Text = "Price:";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(661, 100);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 14);
			this.label3.TabIndex = 5;
			this.label3.Text = "Count:";
			// 
			// CountNumericUpDown
			// 
			this.CountNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CountNumericUpDown.Location = new System.Drawing.Point(705, 98);
			this.CountNumericUpDown.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
			this.CountNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.CountNumericUpDown.Name = "CountNumericUpDown";
			this.CountNumericUpDown.Size = new System.Drawing.Size(200, 22);
			this.CountNumericUpDown.TabIndex = 4;
			this.CountNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// BuyRadioButton
			// 
			this.BuyRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BuyRadioButton.AutoSize = true;
			this.BuyRadioButton.Checked = true;
			this.BuyRadioButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.BuyRadioButton.Location = new System.Drawing.Point(704, 126);
			this.BuyRadioButton.Name = "BuyRadioButton";
			this.BuyRadioButton.Size = new System.Drawing.Size(44, 18);
			this.BuyRadioButton.TabIndex = 6;
			this.BuyRadioButton.TabStop = true;
			this.BuyRadioButton.Text = "Buy";
			this.BuyRadioButton.UseVisualStyleBackColor = true;
			// 
			// SellRadioButton
			// 
			this.SellRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SellRadioButton.AutoSize = true;
			this.SellRadioButton.ForeColor = System.Drawing.Color.Red;
			this.SellRadioButton.Location = new System.Drawing.Point(859, 126);
			this.SellRadioButton.Name = "SellRadioButton";
			this.SellRadioButton.Size = new System.Drawing.Size(46, 18);
			this.SellRadioButton.TabIndex = 7;
			this.SellRadioButton.Text = "Sell";
			this.SellRadioButton.UseVisualStyleBackColor = true;
			// 
			// ActionDateTimePicker
			// 
			this.ActionDateTimePicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ActionDateTimePicker.CustomFormat = "yyyy/MM/dd";
			this.ActionDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.ActionDateTimePicker.Location = new System.Drawing.Point(705, 151);
			this.ActionDateTimePicker.Name = "ActionDateTimePicker";
			this.ActionDateTimePicker.Size = new System.Drawing.Size(200, 22);
			this.ActionDateTimePicker.TabIndex = 8;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(659, 128);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(43, 14);
			this.label4.TabIndex = 9;
			this.label4.Text = "Action:";
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(666, 157);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(36, 14);
			this.label5.TabIndex = 10;
			this.label5.Text = "Date:";
			// 
			// TradesDataGridView
			// 
			this.TradesDataGridView.AllowUserToAddRows = false;
			this.TradesDataGridView.AllowUserToDeleteRows = false;
			this.TradesDataGridView.AllowUserToResizeRows = false;
			this.TradesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TradesDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.TradesDataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.TradesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.TradesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SymbolColumn,
            this.PriceColumn,
            this.CountColumn,
            this.TotalPriceColumn,
            this.ActionColumn,
            this.TimeColumn});
			this.TradesDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.TradesDataGridView.Location = new System.Drawing.Point(12, 32);
			this.TradesDataGridView.MultiSelect = false;
			this.TradesDataGridView.Name = "TradesDataGridView";
			this.TradesDataGridView.ReadOnly = true;
			this.TradesDataGridView.RowHeadersVisible = false;
			this.TradesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.TradesDataGridView.ShowCellToolTips = false;
			this.TradesDataGridView.ShowEditingIcon = false;
			this.TradesDataGridView.Size = new System.Drawing.Size(624, 531);
			this.TradesDataGridView.TabIndex = 11;
			this.TradesDataGridView.SelectionChanged += new System.EventHandler(this.TradesDataGridView_SelectionChanged);
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(661, 16);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(38, 14);
			this.label6.TabIndex = 12;
			this.label6.Text = "Stock:";
			// 
			// StockFilterTextBox
			// 
			this.StockFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.StockFilterTextBox.Location = new System.Drawing.Point(705, 13);
			this.StockFilterTextBox.Name = "StockFilterTextBox";
			this.StockFilterTextBox.Size = new System.Drawing.Size(200, 22);
			this.StockFilterTextBox.TabIndex = 13;
			this.StockFilterTextBox.TextChanged += new System.EventHandler(this.StockFilterTextBox_TextChanged);
			// 
			// SymbolColumn
			// 
			this.SymbolColumn.DataPropertyName = "symbol";
			this.SymbolColumn.HeaderText = "Symbol";
			this.SymbolColumn.Name = "SymbolColumn";
			this.SymbolColumn.ReadOnly = true;
			this.SymbolColumn.Width = 71;
			// 
			// PriceColumn
			// 
			this.PriceColumn.DataPropertyName = "price";
			this.PriceColumn.HeaderText = "Price";
			this.PriceColumn.Name = "PriceColumn";
			this.PriceColumn.ReadOnly = true;
			this.PriceColumn.Width = 58;
			// 
			// CountColumn
			// 
			this.CountColumn.DataPropertyName = "count";
			this.CountColumn.HeaderText = "Count";
			this.CountColumn.Name = "CountColumn";
			this.CountColumn.ReadOnly = true;
			this.CountColumn.Width = 63;
			// 
			// TotalPriceColumn
			// 
			this.TotalPriceColumn.DataPropertyName = "total_price";
			this.TotalPriceColumn.HeaderText = "TotalPrice";
			this.TotalPriceColumn.Name = "TotalPriceColumn";
			this.TotalPriceColumn.ReadOnly = true;
			this.TotalPriceColumn.Width = 85;
			// 
			// ActionColumn
			// 
			this.ActionColumn.DataPropertyName = "action";
			this.ActionColumn.HeaderText = "Action";
			this.ActionColumn.Name = "ActionColumn";
			this.ActionColumn.ReadOnly = true;
			this.ActionColumn.Width = 65;
			// 
			// TimeColumn
			// 
			this.TimeColumn.DataPropertyName = "time";
			this.TimeColumn.HeaderText = "Time";
			this.TimeColumn.Name = "TimeColumn";
			this.TimeColumn.ReadOnly = true;
			this.TimeColumn.Width = 59;
			// 
			// TotalCheckBox
			// 
			this.TotalCheckBox.AutoSize = true;
			this.TotalCheckBox.Location = new System.Drawing.Point(12, 8);
			this.TotalCheckBox.Name = "TotalCheckBox";
			this.TotalCheckBox.Size = new System.Drawing.Size(53, 18);
			this.TotalCheckBox.TabIndex = 14;
			this.TotalCheckBox.Text = "Total";
			this.TotalCheckBox.UseVisualStyleBackColor = true;
			this.TotalCheckBox.CheckedChanged += new System.EventHandler(this.TotalCheckBox_CheckedChanged);
			// 
			// DeleteButton
			// 
			this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeleteButton.Enabled = false;
			this.DeleteButton.Location = new System.Drawing.Point(12, 569);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = new System.Drawing.Size(75, 23);
			this.DeleteButton.TabIndex = 15;
			this.DeleteButton.Text = "Delete";
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = new System.Drawing.Point(830, 179);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(75, 23);
			this.SaveButton.TabIndex = 16;
			this.SaveButton.Text = "Save";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// ClientForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(917, 604);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.DeleteButton);
			this.Controls.Add(this.TotalCheckBox);
			this.Controls.Add(this.StockFilterTextBox);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.TradesDataGridView);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.ActionDateTimePicker);
			this.Controls.Add(this.SellRadioButton);
			this.Controls.Add(this.BuyRadioButton);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.CountNumericUpDown);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.PriceNumericUpDown);
			this.Controls.Add(this.StocksComboBox);
			this.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ClientForm";
			this.ShowIcon = false;
			this.Text = "Client";
			((System.ComponentModel.ISupportInitialize)(this.PriceNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CountNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TradesDataGridView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox StocksComboBox;
		private System.Windows.Forms.NumericUpDown PriceNumericUpDown;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown CountNumericUpDown;
		private System.Windows.Forms.RadioButton BuyRadioButton;
		private System.Windows.Forms.RadioButton SellRadioButton;
		private System.Windows.Forms.DateTimePicker ActionDateTimePicker;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.DataGridView TradesDataGridView;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox StockFilterTextBox;
		private System.Windows.Forms.DataGridViewTextBoxColumn SymbolColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn PriceColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn CountColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn TotalPriceColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn ActionColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn TimeColumn;
		private System.Windows.Forms.CheckBox TotalCheckBox;
		private System.Windows.Forms.Button DeleteButton;
		private System.Windows.Forms.Button SaveButton;
	}
}

