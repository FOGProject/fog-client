/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2017 FOG Project
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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MetroFramework.Components;
using Newtonsoft.Json.Linq;
using Zazzles;
using Zazzles.Data;

namespace FOG
{
    public partial class MainForm : Form
    {
        private const string LogName = "Shutdown GUI";
        private const int OSXTitleBarHeight = 22;
        private const string ProgressColor = "#00567a";

        // Provide 5 different delay options, starting at 15 minutes
        private const int NumberOfDelays = 5;
        private const int StartingDelay = 15;

        private readonly int _gracePeriod = 600;
        private readonly dynamic _transport;
        private readonly Power.ShutdownOptions _options;
        private readonly int _aggregatedDelayTime;
        public MainForm(string[] args)
        {
#if DEBUG
#else
            if (args.Length == 0)
                Environment.Exit(1);
#endif
            Log.Output = Log.Mode.Quiet;

#if DEBUG

            _transport = new JObject();
            _transport.options = Power.ShutdownOptions.Abort.ToString();
            _transport.aggregatedDelayTime = 0;
            _transport.period = 6000;
#else
            _transport = JObject.Parse(Transform.DecodeBase64(args[0]));
#endif
            // Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("nl-BE");
            InitializeComponent();

            // Retrieve what configuration the prompt should use
            _options = Enum.Parse(typeof(Power.ShutdownOptions), _transport.options.ToString());

            btnAbort.Text = (_options == Power.ShutdownOptions.Abort)
                ? Strings.CANCEL
                : Strings.HIDE;

            _aggregatedDelayTime = _transport.aggregatedDelayTime;

            if (_transport.period == null)
                return;
            _gracePeriod = _transport.period;

            Log.Entry(LogName, _gracePeriod.ToString());
            if (_gracePeriod == 0)
                return;

            textBox1.Text = GenerateMessage();
            textBox1.Select(0, 0);

            progressBar1.Maximum = _gracePeriod - 1;
            label1.Text = Time.FormatSeconds(_gracePeriod);

            SetColors();
            Localize();
            SwapBanner();
            GenerateDelays();
            PositionForm();
            this.ActiveControl = labelPostpone;

            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Power, onPower);

        }

        private void Localize()
        {
            labelPostpone.Text = Strings.POSTPONE_FOR;
            var size = labelPostpone.CreateGraphics().MeasureString(labelPostpone.Text, this.Font);
            labelPostpone.Location = new Point((int)(comboPostpone.Location.X - size.Width - 25), labelPostpone.Location.Y);

            btnPostpone.Text = Strings.POSTPONE;

            // The NOW buttons are really long in a lot of languages
            // so account for that length
            btnNow.Text = Strings.SHUTDOWN_NOW;
            size = labelPostpone.CreateGraphics().MeasureString(btnNow.Text, this.Font);
            if (size.Width > btnNow.Width - 25)
            {
                // gotta move everything over
                var offset = size.Width - btnNow.Width + 25;
                btnAbort.Location = new Point((int) (btnAbort.Location.X - offset), btnAbort.Location.Y);
                btnPostpone.Location = new Point((int)(btnPostpone.Location.X - offset), btnPostpone.Location.Y);
                btnNow.Location = new Point((int)(btnNow.Location.X - offset), btnNow.Location.Y);
                btnNow.Width = (int) (size.Width + 25);
            }
        }

        private void SetColors()
        {
            var customColor = ColorTranslator.FromHtml(ProgressColor);
            try
            {
                if (!string.IsNullOrEmpty(Settings.Get("Color")))
                    customColor = ColorTranslator.FromHtml(Settings.Get("Color"));
            }
            catch (Exception)
            {

            }

            MetroStyleManager.Styles.AddStyle("Custom", customColor);
            progressBar1.Style = "Custom";
            comboPostpone.Style = "Custom";
        }

        private void SwapBanner()
        {
            var bannerPath = Path.Combine(Settings.Location, "banner.png");
            if (!File.Exists(bannerPath))
                return;
            var bannerHash = Hash.SHA512(bannerPath);
            if (bannerHash != Settings.Get("BannerHash"))
                return;

            bannerBox.Image = Image.FromFile(bannerPath);
        }

        private void GenerateDelays()
        {
            var delays = new SortedDictionary<int, string>();
            var currentDelay = StartingDelay;

            for (var i = 0; i < NumberOfDelays; i++)
            {
                if (currentDelay + _aggregatedDelayTime > Power.MaxDelayTime)
                    break;

                var readableTime = Time.FormatMinutes(currentDelay,
                    Strings.HOUR, Strings.HOURS, Strings.MINUTE, Strings.MINUTES, Strings.SECOND, Strings.SECONDS);
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
            var company = Settings.Get("Company");
            if (string.IsNullOrWhiteSpace(company))
                company = "FOG";

            string message = (_transport.message != null)
                ? _transport.message.ToString()
                : string.Format(Strings.DEFAULT_MESSAGE, company);

            return message;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (progressBar1.Value >= progressBar1.Maximum)
                Environment.Exit(0);
            progressBar1.Value++;
            progressBar1.Update();
            label1.Text = Time.FormatSeconds(_gracePeriod - progressBar1.Value, 
                Strings.HOUR, Strings.HOURS, Strings.MINUTE, Strings.MINUTES, Strings.SECOND, Strings.SECONDS);
        }

        private void BtnNowClick(object sender, EventArgs e)
        {
            _transport.action = "now";
            Bus.Emit(Bus.Channel.Power, _transport, true);
            Environment.Exit(0);
        }

        private void BtnAbortClick(object sender, EventArgs e)
        {
            if (_options != Power.ShutdownOptions.Abort)
                Environment.Exit(0);

            _transport.action = "abort";
            Bus.Emit(Bus.Channel.Power, _transport, true);
            Environment.Exit(1);
        }

        private void BtnPostponeClick(object sender, EventArgs e)
        {
            _transport.action = "delay";
            _transport.delay = comboPostpone.SelectedValue;
            Bus.Emit(Bus.Channel.Power, _transport, true);
            Environment.Exit(1);
        }

        private void onPower(dynamic data)
        {
            if (data.action == null) return;

            if (data.action.ToString() == "abort" || data.action.ToString() == "delay")
                Environment.Exit(2);
        }
    }
}
