/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
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
using System.Threading;
using System.Windows.Forms;
using MetroFramework.Controls;
using Zazzles;
using Zazzles.Data;

namespace FOG
{
    public partial class GUI : Form
    {
        public bool Success = false;
        private volatile bool _logicClick = false;
        private bool _dragging = false;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;
        private Thread _installThread;

        private enum InstallSection
        {
            Prepare,
            Install,
            Configure,
            Secure
        }

        public GUI()
        {
            InitializeComponent();
            UpdateWelcomeText();
            title.Text += " v" + Helper.ClientVersion;
            if (Settings.OS == Settings.OSType.Windows)
                logSwitch.Checked = true;

            Bus.Subscribe(Bus.Channel.Log, OnLog);

            _installThread = new Thread(InstallClient)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "client-install"
            };
        }

        public void InstallClient()
        {
            if (!UpdateSection(busyWorkLabel, busyWorkSpinner, InstallSection.Prepare))
                return;

            if (!UpdateSection(installFileLabel, filesSpinner, InstallSection.Install))
                return;

            if (!UpdateSection(configuringLabel, configSpinner, InstallSection.Configure))
                return;

            if (!UpdateSection(encryptLabel, encryptionSpinner, InstallSection.Secure))
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
            if (Settings.OS == Settings.OSType.Windows)
            {
                return RSA.FOGProjectCertificate() != null && RSA.ServerCertificate() != null;
            }

            Helper.PinServerCert();
            Helper.PinFOGCert();
            return true;
        }

        private bool Configure()
        {
            Helper.SaveSettings("0", "0", addressTxtBox.Text, webRootTxtBox.Text, "FOG", 
                (logSwitch.Checked) ? "1" : "0");
            Helper.Instance.Configure();
            return true;
        }


        private bool UpdateSection(Label label, MetroProgressSpinner spinner, InstallSection section)
        {
            label.ForeColor = System.Drawing.Color.Black;

            spinner.Invoke((MethodInvoker)(() =>
            {
                spinner.Value = 20;
            }));

            var success = false;
            try
            {
                switch (section)
                {
                    case InstallSection.Prepare:
                        success = Helper.Instance.PrepareFiles();
                        break;
                    case InstallSection.Install:
                        success = Helper.Instance.Install("0", "0", 
                            addressTxtBox.Text, webRootTxtBox.Text, "FOG",
                            (logSwitch.Checked) ? "1" : "0", null);
                        break;
                    case InstallSection.Configure:
                        success = Configure();
                        break;
                    case InstallSection.Secure:
                        success = InstallCerts();
                        break;
                }
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

        private void FormLoad(object sender, EventArgs e)
        {
            this.MinimumSize = new System.Drawing.Size(this.Width, this.Height);

            this.MaximumSize = new System.Drawing.Size(this.MinimumSize.Width + 500, this.MinimumSize.Height + 500);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }
    }
}
