
using System;
using System.Drawing;
using System.Windows.Forms;
using FOG.Handlers;
using FOG.Handlers.Data;
using FOG.Handlers.Power;
using Newtonsoft.Json.Linq;

namespace FOG {
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
    {
		private int _gracePeriod = 60;
	    private int delayTime = 10;
	    private readonly string LogName = "Shutdown GUI";

		public MainForm(string[] args)
		{
		    try
		    {
		        Log.FilePath = "ShutdownLog.txt";
		        Log.Entry(LogName, "Outputted data");

                InitializeComponent();


		        var message = "";

                Log.Entry(LogName, "Step 1");

                Log.Entry(LogName, _gracePeriod.ToString());
		        if (_gracePeriod == 0)
		            throw new Exception("Invaid gracePeriod");
		        //
		        // The InitializeComponent() call is required for Windows Forms designer support.
		        //
                Log.Entry(LogName, "Step 2");



		        //Generate the message
		        message += "Please save all work and close programs.";

		        if (btnAbort.Enabled && btnAbort.Text.Contains("Abort"))
		            message += " Press Abort to cancel.";
		        else if (btnAbort.Enabled && btnAbort.Text.Contains("Delay"))
		            message += " You may only delay this operation once.";

		        textBox1.Text = message;
		        textBox1.Select(0, 0);

                Log.Entry(LogName, "Step 3");

		        progressBar1.Maximum = _gracePeriod - 1;
		        label1.Text = _gracePeriod + " seconds";
		        var workingArea = Screen.GetWorkingArea(this);
		        Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);
                Log.Entry(LogName, "Step 4");

		        Bus.SetMode(Bus.Mode.Client);
		        Bus.Subscribe(Bus.Channel.Power, onAbort);
		    }
            catch (Exception ex)
		    {
               Log.Error(LogName, "Unable to start GUI");
               Log.Error(LogName, ex);
               Environment.Exit(1);
		    }
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
		
		void BtnAbortClick(object sender, EventArgs e)
		{
            Environment.Exit(1);
		}

	    void onAbort(dynamic data)
	    {
	        if (data.action == null) return;

            if(data.action.ToString().Equals("abort"))
                Environment.Exit(2);
            else if (data.action.ToString().Equals("delay"))
                Environment.Exit(2);
	    }
	}
}
