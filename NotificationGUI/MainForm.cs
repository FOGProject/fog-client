
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FOG.Handlers;
using FOG.Handlers.Power;
using Newtonsoft.Json.Linq;

namespace FOG {
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form {
		
		private int _gracePeriod;
	    private int delayTime = 10;
	    private dynamic data;

		public MainForm(string[] args)
		{

		    if (args.Length < 0) return;

		    try
		    {
		        data = JObject.Parse(args[0]);
		    }
		    catch (Exception)
		    {
		        return;
		    }

		    var options = (Power.FormOption) Enum.Parse(typeof (Power.FormOption), data.options.ToString());

		    switch (options)
		    {
		        case Power.FormOption.None:
		            btnAbort.Enabled = false;
		            break;
		        case Power.FormOption.Delay:
                    btnAbort.Text = "Delay " + delayTime + " Minutes";
		            break;
		    }

		    var message = "";

		    try
		    {
		        if (data.gracePeriod == null) return;
		        _gracePeriod = (int) data.gracePeriod;

		        message = data.message.ToString();
		    }
		    catch (Exception)
		    {
                message = "This computer needs to perform maintenance.";
		    }

		    if (_gracePeriod == 0)
		        return;
		    //
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		        

			//Generate the message
		    message += " Please save all work and close programs.";

		    if (btnAbort.Enabled && btnAbort.Text.Contains("Abort"))
		        message += " Press Abort to cancel.";
            else if (btnAbort.Enabled && btnAbort.Text.Contains("Delay"))
                message += " You may only delay this operation once.";

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
		
		void BtnAbortClick(object sender, EventArgs e) {
            dynamic json = new JObject();

		    json.action = (btnAbort.Text.StartsWith("Delay")) ? "delay" : "abort";
		    json.delay = delayTime;
		    json.gracePeriod = _gracePeriod;

            Bus.Emit(Bus.Channel.Power, json, true);
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
