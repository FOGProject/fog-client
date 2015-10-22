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
using Newtonsoft.Json.Linq;
using Zazzles;
using Zazzles.Data;

namespace FOG
{
    /// <summary>
    ///     Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly int _gracePeriod = 60;
        private readonly int delayTime = 10;
        private readonly string LogName = "Shutdown GUI";
        private readonly dynamic transport = new JObject();

        public MainForm(string[] args)
        {
            try
            {
                Log.Output = Log.Mode.Quiet;

                if (args.Length < 0) Environment.Exit(1);
                Log.Entry(LogName, args[0]);
                var arg = Transform.DecodeBase64(args[0]);

                transport = JObject.Parse(arg);

                Log.Entry(LogName, transport.ToString());
                Log.Entry(LogName, transport.command.ToString());

                InitializeComponent();

                var options = (Power.ShutdownOptions) Enum.Parse(typeof (Power.ShutdownOptions), transport.options.ToString());

                switch (options)
                {
                  case Power.ShutdownOptions.None:
                      btnAbort.Enabled = false;
                      break;
                  case Power.ShutdownOptions.Delay:
                      btnAbort.Text = "Delay " + delayTime + " Minutes";
                      break;
                }

                string message;

                try
                {
                    if (transport.period == null) return;
                    _gracePeriod = (int) transport.period;

                    message = transport.message.ToString();
                }
                catch (Exception)
                {
                }
                message = "This computer needs to perform maintenance.";

                Log.Entry(LogName, _gracePeriod.ToString());
                if (_gracePeriod == 0)
                    throw new Exception("Invaid gracePeriod");


                //Generate the message
                message += " Please save all work and close programs.";

                if (btnAbort.Enabled && btnAbort.Text.Contains("Abort"))
                    message += " Press Abort to cancel.";
                else if (btnAbort.Enabled && btnAbort.Text.Contains("Delay"))
                    message += " You may only delay this operation once.";

                textBox1.Text = message;
                textBox1.Select(0, 0);

                progressBar1.Maximum = _gracePeriod - 1;
                label1.Text = _gracePeriod + " seconds";
                var workingArea = Screen.GetWorkingArea(this);
                var height = workingArea.Bottom - Size.Height;
                if (Settings.OS == Settings.OSType.Mac) height = height - 22;

                Location = new Point(workingArea.Right - Size.Width, height);
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

        private void TimerTick(object sender, EventArgs e)
        {
            if (progressBar1.Value >= progressBar1.Maximum)
                Environment.Exit(0);
            progressBar1.Value++;
            progressBar1.Update();
            label1.Text = (_gracePeriod - progressBar1.Value) + " seconds";
        }

        private void BtnNowClick(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void BtnAbortClick(object sender, EventArgs e)
        {
            transport.action = (btnAbort.Text.StartsWith("Delay")) ? "delay" : "abort";
            transport.delay = delayTime;
            Bus.Emit(Bus.Channel.Power, transport, true);
            Environment.Exit(1);
        }

        private void onAbort(dynamic data)
        {
            if (data.action == null) return;

            if (data.action.ToString().Equals("abort"))
                Environment.Exit(2);
            else if (data.action.ToString().Equals("delay"))
                Environment.Exit(2);
        }
    }
}
