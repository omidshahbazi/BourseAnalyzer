namespace BourseAnalyzerClient
{
	partial class LoginForm
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
			this.UsernameTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.PasswordTextBox = new System.Windows.Forms.TextBox();
			this.LoginButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// UsernameTextBox
			// 
			this.UsernameTextBox.Location = new System.Drawing.Point(74, 12);
			this.UsernameTextBox.Name = "UsernameTextBox";
			this.UsernameTextBox.Size = new System.Drawing.Size(250, 20);
			this.UsernameTextBox.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Username:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Password:";
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Location = new System.Drawing.Point(74, 38);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.ShortcutsEnabled = false;
			this.PasswordTextBox.Size = new System.Drawing.Size(250, 20);
			this.PasswordTextBox.TabIndex = 2;
			this.PasswordTextBox.UseSystemPasswordChar = true;
			// 
			// LoginButton
			// 
			this.LoginButton.Location = new System.Drawing.Point(15, 77);
			this.LoginButton.Name = "LoginButton";
			this.LoginButton.Size = new System.Drawing.Size(309, 23);
			this.LoginButton.TabIndex = 4;
			this.LoginButton.Text = "Login";
			this.LoginButton.UseVisualStyleBackColor = true;
			this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
			// 
			// LoginForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(341, 113);
			this.Controls.Add(this.LoginButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.UsernameTextBox);
			this.Name = "LoginForm";
			this.ShowIcon = false;
			this.Text = "Login";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox UsernameTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox PasswordTextBox;
		private System.Windows.Forms.Button LoginButton;
	}
}

