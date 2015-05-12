
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FOG.Handlers;

namespace FOG {
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form {
		
		private int gracePeriod;
		
		public MainForm(IEnumerable<string> args) {
			setGracePeriod();
		    if (gracePeriod == 0)
		        Environment.Exit(0);
		    //
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			
			//Generate the message
			var message = "This computer needs to ";
			var reason = ".";
			
			foreach(var arg in args) {
			    if (arg.Contains("noAbort"))
			        btnAbort.Enabled = false;
			    else if (arg.Contains("shutdown"))
			        btnNow.Text = "Shutdown Now";
			    else if (arg.Contains("reboot"))
			        btnNow.Text = "Reboot Now";
			    else if (arg.Contains("snapin"))
			        reason = " to apply new software.";
			    else if (arg.Contains("task"))
			        reason = " to perform a task.";
			}

		    message = btnNow.Text.Contains("Shutdown") ? message + "shutdown" : message + "reboot";

		    message = message + reason + " Please save all work and close programs.";


		    if (btnAbort.Enabled)
		        message = message + " Press Abort to cancel.";

		    textBox1.Text = message;
			textBox1.Select(0,0);

			progressBar1.Maximum = gracePeriod-1;		
			label1.Text = gracePeriod + " seconds";
			var workingArea = Screen.GetWorkingArea(this);
			Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);
		}
		
		private void setGracePeriod() {
			var regValue = RegistryHandler.GetSystemSetting("NotificationPromptTime");
			gracePeriod = 60;
		    if (regValue == null) return;
		    
            try {
		        gracePeriod = int.Parse(regValue);
		    } catch (Exception) {
		        gracePeriod = 60;
		    }
		}
		
		//Prevent the window from being moved
		//http://stackoverflow.com/a/907868
		protected override void WndProc(ref Message message) {
		    const int WM_SYSCOMMAND = 0x0112;
		    const int SC_MOVE = 0xF010;
		
		    switch(message.Msg) {
		        case WM_SYSCOMMAND:
		           var command = message.WParam.ToInt32() & 0xfff0;
		           if (command == SC_MOVE)
		              return;
		           break;
		    }
		
		    base.WndProc(ref message);
		}		
		
		void TimerTick(object sender, EventArgs e) {
		    if (progressBar1.Value >= progressBar1.Maximum)
		        Environment.Exit(0);
		    progressBar1.Value++;
			progressBar1.Update();
			label1.Text = (gracePeriod-progressBar1.Value) + " seconds";
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
