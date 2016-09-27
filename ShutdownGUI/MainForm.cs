/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2016 FOG Project
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Zazzles;
using Zazzles.Data;
using Zazzles.DataContracts;

namespace FOG
{
    public partial class MainForm : Form
    {
        private const string LogName = "Shutdown GUI";
        private const int OSXTitleBarHeight = 22;

        // Provide 5 different delay options, starting at 15 minutes
        private const int NumberOfDelays = 5;
        private const int StartingDelay = 15;

        private readonly PowerAction _action;

        public MainForm(string[] args)
        {
            if (args.Length == 0) Environment.Exit(1);
            Log.Output = Log.Mode.Quiet;

            dynamic transport = JObject.Parse(Transform.DecodeBase64(args[0]));
            _action = transport.Data.ToObject<PowerDelayRequest>();

            InitializeComponent();

            // Retrieve what configuration the prompt should use
            btnAbort.Text = (_action.Option == Power.ShutdownOptions.Abort)
                ? "Cancel"
                : "Hide";

            switch (_action.Type)
            {
                case Power.Actions.Shutdown:
                    btnNow.Text = "Shutdown Now";
                    break;
                case Power.Actions.Restart:
                    btnNow.Text = "Restart Now";
                    break;
                default:
                    throw new Exception("Unsupported action");
            }

            Log.Entry(LogName, _action.PromptTime.ToString());
            if (_action.PromptTime == 0)
                throw new Exception("Invaid gracePeriod");

            textBox1.Text = GenerateMessage();
            textBox1.Select(0, 0);

            progressBar1.Maximum = _action.PromptTime - 1;
            label1.Text = Time.FormatSeconds(_action.PromptTime);

            GenerateDelays();
            PositionForm();

            Bus.Mode = Bus.Role.Client;
            Bus.Subscribe(Bus.Channel.Power, onPower);
        }

        private void GenerateDelays()
        {
            var delays = new SortedDictionary<int, string>();
            var currentDelay = StartingDelay;

            for (var i = 0; i < NumberOfDelays; i++)
            {
                if (currentDelay + _action.AggregatedDelayTime > Power.MaxDelayTime)
                    break;

                var readableTime = Time.FormatMinutes(currentDelay);
                // Delays are processed in minutes
                delays.Add(currentDelay, readableTime);

                // Double the delay for the next increment
                currentDelay = currentDelay*2;
            }

            if (delays.Count == 0)
            {
                comboPostpone.Enabled = false;
                btnPostpone.Enabled = false;
                return;
            }

            comboPostpone.DataSource = new BindingSource(delays, null);
            comboPostpone.DisplayMember = "Value";
            comboPostpone.ValueMember = "Key";

        }

        private void PositionForm()
        {
            var workingArea = Screen.GetWorkingArea(this);
            var height = workingArea.Bottom - Size.Height;

            // Account for the title bar on OSX which offsets the height
            if (Settings.OS == Settings.OSType.Mac) height = height - OSXTitleBarHeight;

            Location = new Point(workingArea.Right - Size.Width, height);
        }

        private string GenerateMessage()
        {
            var message = (!string.IsNullOrWhiteSpace(_action.Message))
                ? _action.Message
                : "This computer needs to perform maintenance.";
            message += " Please save all work and close programs.";

            return message;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (progressBar1.Value >= progressBar1.Maximum)
                Environment.Exit(0);
            progressBar1.Value++;
            progressBar1.Update();
            label1.Text = Time.FormatSeconds(_action.PromptTime - progressBar1.Value);
        }

        private void BtnNowClick(object sender, EventArgs e)
        {
            var executeNowRequest = new PowerExecuteNowRequest();
            Bus.Emit(Bus.Channel.Power, executeNowRequest, true);
            Environment.Exit(0);
        }

        private void BtnAbortClick(object sender, EventArgs e)
        {
            if (_action.Option != Power.ShutdownOptions.Abort)
                Environment.Exit(0);

            var abortRequest = new PowerAbortRequest();
            Bus.Emit(Bus.Channel.Power, abortRequest, true);
            Environment.Exit(1);
        }

        private void BtnPostponeClick(object sender, EventArgs e)
        {
            var delayRequest = new PowerDelayRequest()
            {
                Delay = (int) comboPostpone.SelectedValue
            };

            Bus.Emit(Bus.Channel.PowerRequest, delayRequest, true);
            Environment.Exit(1);
        }

        private void onPower(dynamic data)
        {
            if (data.action == null)
                return;

            Power.BusCommands action = Enum.Parse(typeof(Power.BusCommands), data.action.ToString(), true);

            switch (action)
            {
                case Power.BusCommands.Abort:
                case Power.BusCommands.Delay:
                    Environment.Exit(2);
                    break;
                default:
                    break;
            }
        }
    }
}
