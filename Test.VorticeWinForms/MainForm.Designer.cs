namespace Test.VorticeWinForms
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.directxCtrl1 = new DirectXCtrl();
			this.SuspendLayout();
			// 
			// directxCtrl1
			// 
			this.directxCtrl1.BackColor = SystemColors.ControlDarkDark;
			this.directxCtrl1.Dock = DockStyle.Fill;
			this.directxCtrl1.Location = new Point(0, 0);
			this.directxCtrl1.Name = "directxCtrl1";
			this.directxCtrl1.Size = new Size(778, 744);
			this.directxCtrl1.TabIndex = 0;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(778, 744);
			Controls.Add(this.directxCtrl1);
			Name = "MainForm";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Test Vortice WinForms";
			this.ResumeLayout(false);
		}

		#endregion

		private DirectXCtrl directxCtrl1;
	}
}
