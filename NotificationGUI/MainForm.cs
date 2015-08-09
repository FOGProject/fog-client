/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2015 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

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
	public partial class MainForm : Form {
		
		private int _gracePeriod = 60;
	    private int delayTime = 10;
	    private readonly string LogName = "Shutdown GUI";
	    private dynamic transport = new JObject();

		public MainForm(string[] args)
		{
		    try
		    {
		        Log.FilePath = "ShutdownLog.txt";

		        if (args.Length < 0) Environment.Exit(1);
		        Log.Entry(LogName, args[0]);
		        var arg = Transform.DecodeBase64(args[0]);

		        transport = JObject.Parse(arg);

		        Log.Entry(LogName, transport.ToString());
		        Log.Entry(LogName, transport.command.ToString());
		        Log.Entry(LogName, "Outputted data");

                InitializeComponent();


		        var options = (Power.FormOption) Enum.Parse(typeof (Power.FormOption), transport.options.ToString());

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

                Log.Entry(LogName, "Step 1");
		        try
		        {
                    if (transport.period == null) return;
                    _gracePeriod = (int)transport.period;

		            message = transport.message.ToString();
		        }
		        catch (Exception)
		        {
		            message = "This computer needs to perform maintenance.";
		        }

                Log.Entry(LogName, _gracePeriod.ToString());
		        if (_gracePeriod == 0)
		            throw new Exception("Invaid gracePeriod");
		        //
		        // The InitializeComponent() call is required for Windows Forms designer support.
		        //
                Log.Entry(LogName, "Step 2");



		        //Generate the message
		        message += " Please save all work and close programs.";

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
            transport.action = (btnAbort.Text.StartsWith("Delay")) ? "delay" : "abort";
		    transport.delay = delayTime;
            Bus.Emit(Bus.Channel.Power, transport, true);
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
