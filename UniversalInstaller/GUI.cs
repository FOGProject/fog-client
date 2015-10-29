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
using System.Net;
using System.Threading;
using System.Windows.Forms;
using MetroFramework.Controls;
using Zazzles;

namespace FOG
{
    public partial class GUI : Form
    {
        public bool Success = false;
        private volatile bool _logicClick = false;
        private bool _dragging = false;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;
        private Thread _serverUpThread;
        private Thread _installThread;
        private volatile bool _checkServer = false;

        public GUI()
        {
            InitializeComponent();
            UpdateWelcomeText();
            Bus.Subscribe(Bus.Channel.Log, OnLog);

            _serverUpThread = new Thread(UpdateSpinner)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "server-status"
            };

            _installThread = new Thread(InstallClient)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "client-install"
            };
        }

        public void InstallClient()
        {
            if (!UpdateSection(busyWorkLabel, busyWorkSpinner, Helper.Instance.PrepareFiles))
                return;

            if (!UpdateSection(installFileLabel, filesSpinner, Helper.Instance.Install))
                return;

            if (!UpdateSection(configuringLabel, configSpinner, Configure))
                return;

            if (!UpdateSection(encryptLabel, encryptionSpinner, InstallCerts))
                return;

            this.nextButton.Invoke((MethodInvoker)(() =>
            {
                nextButton.Enabled = true;
            }));
        }

        private void OnLog(dynamic data)
        {
            this.logBox.Text += data.message;
        }

        private bool InstallCerts()
        {
            Helper.PinServerCert();
            Helper.PinFOGCert();
            return true;
        }

        private bool Configure()
        {
            Helper.SaveSettings((
                httpsSwitch.Checked) ? "1" : "0", 
                "0", addressTxtBox.Text, webRootTxtBox.Text, "FOG", 
                (logSwitch.Checked) ? "1" : "0");
            Helper.Instance.Configure();
            return true;
        }


        private bool UpdateSection(Label label, MetroProgressSpinner spinner, Func<bool> method)
        {
            label.ForeColor = System.Drawing.Color.Black;

            spinner.Invoke((MethodInvoker)(() =>
            {
                spinner.Value = 20;
            }));

            var success = false;
            try
            {
                success = method.Invoke();
            }
            catch (Exception ex)
            {
                logBox.Invoke((MethodInvoker)(() =>
                {
                    logBox.AppendText(ex.Message);
                }));
                success = false;
            }


            spinner.Invoke((MethodInvoker)(() =>
            {
                spinner.Value = 100;
                spinner.ForeColor = (success) ? Color.ForestGreen : Color.Crimson;
            }));

            return success;
        }

        private void ToggleLogButtonClick(object sender, System.EventArgs e)
        {
            this.logBox.Visible = !this.logBox.Visible;
            this.showLogButton.Text = (!this.logBox.Visible) ? "Show Log" : "Hide Log";
        }

        public void UpdateWelcomeText()
        {
            this.welcomeText.Text = this.welcomeText.Text.Replace("{OPERATING_SYSTEM}", Settings.OS.ToString());
        }

        private void exitButton_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if(!_logicClick)
                e.Cancel = true;
            _logicClick = false;
        }

        private void AgreeBtnOnClick(object sender, EventArgs eventArgs)
        {
            _logicClick = true;
            tabControl.SelectTab(tabControl.SelectedIndex + 1);
        }

        private void InstallBtnOnClick(object sender, EventArgs eventArgs)
        {
            _logicClick = true;
            tabControl.SelectTab(tabControl.SelectedIndex + 1);
            _installThread.Start();
        }

        private void NextBtnOnClick(object sender, EventArgs eventArgs)
        {
            _logicClick = true;
            tabControl.SelectTab(tabControl.SelectedIndex + 1);
            _checkServer = (tabControl.SelectedTab == settingsTab);
            if(_checkServer)
                _serverUpThread.Start();
        }

        private void FinishBtnOnClick(object sender, EventArgs eventArgs)
        {
            Success = true;
            Application.Exit();
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _dragCursorPoint = Cursor.Position;
            _dragFormPoint = this.Location;
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            var dif = Point.Subtract(Cursor.Position, new Size(_dragCursorPoint));
            this.Location = Point.Add(_dragFormPoint, new Size(dif));
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void beginClick(object sender, EventArgs e)
        {
            _logicClick = true;
            tabControl.SelectTab(tabControl.SelectedIndex + 1);
        }

        private void UpdateSpinner()
        {
            while (_checkServer)
            {
                var serverStatus = CheckServer();

                serverStatusSpinner.Invoke((MethodInvoker)(() =>
                {
                    serverStatusSpinner.ForeColor = (serverStatus) ? Color.ForestGreen : Color.Crimson;
                    installBtn.Enabled = (serverStatus);
                }));
                Thread.Sleep(100);
            }
        }

        private void FormLoad(object sender, EventArgs e)
        {
            this.MinimumSize = new System.Drawing.Size(this.Width, this.Height);

            this.MaximumSize = new System.Drawing.Size(this.MinimumSize.Width + 500, this.MinimumSize.Height + 500);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private bool CheckServer()
        {
            using (var client = new HeadClient())
            {
                client.HeadOnly = true;
                try
                {
                    var url = (httpsSwitch.Checked) ? "https://" : "http://";
                    url += addressTxtBox.Text;
                    url += webRootTxtBox.Text;

                    var response = client.DownloadString(url);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }

    class HeadClient : WebClient
    {
        public bool HeadOnly { get; set; }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest req = base.GetWebRequest(address);
            if (HeadOnly && req.Method == "GET")
            {
                req.Method = "HEAD";
            }
            return req;
        }
    }
}
