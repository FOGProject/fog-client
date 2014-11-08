
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FOG {
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form {
		
		private int gracePeriod;
		
		public MainForm(string[] args) {
			setGracePeriod();
			
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			
			//Generate the message
			String message = "This computer needs to ";
			String reason = ".";
			
			foreach(String arg in args) {
				if(arg.Contains("noAbort")) {
					this.btnAbort.Enabled = false;
				}else if(arg.Contains("shutdown")) {
					this.btnNow.Text = "Shutdown Now";
				} else if(arg.Contains("reboot")) {
					this.btnNow.Text = "Reboot Now";
				} else if(arg.Contains("snapin")) {
					reason = " to apply new software.";
				} else if(arg.Contains("task")) {
				   	reason = " to perform a task.";
				}
				          	
			}	
			
			if(this.btnNow.Text.Contains("Shutdown")) {
				message = message + "shutdown";
			} else {
				message = message + "reboot";
			}
			
			message = message + reason + " Please save all work and close programs.";
			
			
			if(this.btnAbort.Enabled) {
				message = message + " Press Abort to cancel.";
			}
			
			this.textBox1.Text = message;
			this.textBox1.Select(0,0);

			this.progressBar1.Maximum = this.gracePeriod-1;		
			this.label1.Text = this.gracePeriod.ToString() + " seconds";
			Rectangle workingArea = Screen.GetWorkingArea(this);
			this.Location = new Point(workingArea.Right - Size.Width, 
			                          workingArea.Bottom - Size.Height);
		}
		
		private void setGracePeriod() {
			String regValue = RegistryHandler.getSystemSetting("NotificationPromptTime");
			this.gracePeriod = 60;
			if(regValue != null) {
				try {
					this.gracePeriod = int.Parse(regValue);
				} catch (Exception) {
					this.gracePeriod = 60;
				}
			}

		}
		
		//Prevent the window from being moved
		//http://stackoverflow.com/a/907868
		protected override void WndProc(ref Message message) {
		    const int WM_SYSCOMMAND = 0x0112;
		    const int SC_MOVE = 0xF010;
		
		    switch(message.Msg) {
		        case WM_SYSCOMMAND:
		           int command = message.WParam.ToInt32() & 0xfff0;
		           if (command == SC_MOVE)
		              return;
		           break;
		    }
		
		    base.WndProc(ref message);
		}		
		
		void TimerTick(object sender, EventArgs e) {
			if(progressBar1.Value >= progressBar1.Maximum) {
				Environment.Exit(0);
			}
			progressBar1.Value++;
			progressBar1.Update();
			this.label1.Text = (this.gracePeriod-this.progressBar1.Value).ToString() + " seconds";
		}
		
		void BtnNowClick(object sender, EventArgs e) {
			Environment.Exit(0);
		}
		
		//Abort button
		void btnAbortClick(object sender, EventArgs e) {
			Environment.Exit(2);
		}
	}
}
