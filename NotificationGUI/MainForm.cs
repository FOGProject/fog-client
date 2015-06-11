
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FOG.Handlers;
using Newtonsoft.Json.Linq;

namespace FOG {
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form {
		
		private int _gracePeriod;
		
		public MainForm(string[] args) {

            foreach (var arg in args.Where(arg => arg.Contains("noAbort")))
                btnAbort.Enabled = false;

		    try
		    {
		        for (var i = 0; i < args.Length; i++)
		        {
		            if (!args[i].Contains("period") || i >= args.Length - 1) continue;
		            _gracePeriod = int.Parse(args[i + 1]);
		            break;
		        }
		    }
		    catch (Exception ex)
		    {
		    }


		    if (_gracePeriod == 0)
		        Environment.Exit(0);
		    //
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			
			//Generate the message
            var message = "This computer needs to perform maintenance.";



		    message = message + " Please save all work and close programs.";


		    if (btnAbort.Enabled)
		        message = message + " Press Abort to cancel.";

		    textBox1.Text = message;
			textBox1.Select(0,0);

			progressBar1.Maximum = _gracePeriod-1;		
			label1.Text = _gracePeriod + " seconds";
			var workingArea = Screen.GetWorkingArea(this);
			Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);

            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Power, onAbort);
		}

		//Prevent the window from being moved
		//http://stackoverflow.com/a/907868
		protected override void WndProc(ref Message message) {
		    const int wmSyscommand = 0x0112;
		    const int scMove = 0xF010;
		
		    switch(message.Msg) {
		        case wmSyscommand:
		           var command = message.WParam.ToInt32() & 0xfff0;
		           if (command == scMove)
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
			label1.Text = (_gracePeriod-progressBar1.Value) + " seconds";
		}
		
		void BtnNowClick(object sender, EventArgs e) {
			Environment.Exit(0);
		}
		
		//Abort button
		void BtnAbortClick(object sender, EventArgs e) {
            Bus.Emit(Bus.Channel.Power, new JObject{ "action", "abort" }, true);
            Environment.Exit(1);
		}

	    void onAbort(dynamic data)
	    {
	        if (data.action == null) return;
            if(data.action.ToString().Equals("abort"))
                Environment.Exit(2);
	    }
	}
}
