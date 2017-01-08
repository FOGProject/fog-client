
using System.Drawing;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Components;
using MetroFramework.Controls;

namespace FOG {
	partial class MainForm {
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnAbort = new MetroFramework.Controls.MetroButton();
            this.progressBar1 = new MetroFramework.Controls.MetroProgressBar();
            this.label1 = new MetroFramework.Controls.MetroLabel();
            this.bannerBox = new System.Windows.Forms.PictureBox();
            this.textBox1 = new MetroFramework.Controls.MetroTextBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.btnNow = new MetroFramework.Controls.MetroButton();
            this.btnPostpone = new MetroFramework.Controls.MetroButton();
            this.labelPostpone = new MetroFramework.Controls.MetroLabel();
            this.comboPostpone = new MetroFramework.Controls.MetroComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.bannerBox)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(281, 283);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(122, 41);
            this.btnAbort.TabIndex = 5;
            this.btnAbort.Text = "Cancel";
            this.btnAbort.Click += new System.EventHandler(this.BtnAbortClick);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 327);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(645, 21);
            this.progressBar1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Window;
            this.label1.Location = new System.Drawing.Point(12, 306);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(250, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "60 seconds";
            // 
            // bannerBox
            // 
            this.bannerBox.Image = ((System.Drawing.Image)(resources.GetObject("bannerBox.Image")));
            this.bannerBox.Location = new System.Drawing.Point(12, 12);
            this.bannerBox.Name = "bannerBox";
            this.bannerBox.Size = new System.Drawing.Size(645, 120);
            this.bannerBox.TabIndex = 3;
            this.bannerBox.TabStop = false;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = this.btnAbort.BackColor;
            this.textBox1.Location = new System.Drawing.Point(12, 138);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(645, 106);
            this.textBox1.TabIndex = 5;
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // btnNow
            // 
            this.btnNow.Location = new System.Drawing.Point(536, 283);
            this.btnNow.Name = "btnNow";
            this.btnNow.Size = new System.Drawing.Size(121, 41);
            this.btnNow.TabIndex = 6;
            this.btnNow.Text = "Restart Now";
            this.btnNow.Click += new System.EventHandler(this.BtnNowClick);
            // 
            // btnPostpone
            // 
            this.btnPostpone.Location = new System.Drawing.Point(409, 283);
            this.btnPostpone.Name = "btnPostpone";
            this.btnPostpone.Size = new System.Drawing.Size(121, 41);
            this.btnPostpone.TabIndex = 7;
            this.btnPostpone.Text = "Postpone";
            this.btnPostpone.Click += new System.EventHandler(this.BtnPostponeClick);
            // 
            // labelPostpone
            // 
            this.labelPostpone.AutoSize = true;
            this.labelPostpone.Location = new System.Drawing.Point(277, 253);
            this.labelPostpone.Name = "labelPostpone";
            this.labelPostpone.Size = new System.Drawing.Size(88, 25);
            this.labelPostpone.TabIndex = 8;
            this.labelPostpone.Text = "Postpone for:";
            // 
            // comboPostpone
            // 
            this.comboPostpone.FormattingEnabled = true;
            this.comboPostpone.ItemHeight = 23;
            this.comboPostpone.Location = new System.Drawing.Point(374, 250);
            this.comboPostpone.Name = "comboPostpone";
            this.comboPostpone.Size = new System.Drawing.Size(282, 29);
            this.comboPostpone.TabIndex = 9;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(668, 358);
            this.ControlBox = false;
            this.Controls.Add(this.comboPostpone);
            this.Controls.Add(this.labelPostpone);
            this.Controls.Add(this.btnPostpone);
            this.Controls.Add(this.btnNow);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.bannerBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnAbort);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.bannerBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private MetroButton btnNow;
		private Timer timer;
		private MetroTextBox textBox1;
		private PictureBox bannerBox;
		private MetroLabel label1;
		private MetroButton btnAbort;
        private MetroProgressBar progressBar1;
        private MetroButton btnPostpone;
        private MetroComboBox comboPostpone;
        private MetroLabel labelPostpone;
    }
}
