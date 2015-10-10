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
using FOG.Core;
using MetroFramework.Controls;

namespace FOG
{
    public partial class GUI : Form
    {
        public bool success = false;
        private volatile bool logicClick = false;
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        private Thread serverUpThread;
        private Thread installThread;

        public GUI()
        {
            InitializeComponent();
            UpdateWelcomeText();

            serverUpThread = new Thread(UpdateSpinner)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "server-status"
            };
            serverUpThread.Start();

            installThread = new Thread(InstallClient)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "client-install"
            };
        }

        public void InstallClient()
        {
            if (!UpdateSection(busyWorkLabel, busyWorkSpinner, GenericSetup.Instance.PrepareFiles))
                return;

            if (!UpdateSection(installFileLabel, filesSpinner, GenericSetup.Instance.Install))
                return;

            if (!UpdateSection(configuringLabel, configSpinner, configure))
                return;

            if (!UpdateSection(encryptLabel, encryptionSpinner, GenericSetup.PinServerCert))
                return;
        }

        private bool configure()
        {
            GenericSetup.SaveSettings((
                httpsSwitch.Checked) ? "1" : "0", 
                "0", addressTxtBox.Text, webRootTxtBox.Text, "FOG", 
                (logSwitch.Checked) ? "1" : "0");
            GenericSetup.Instance.Configure();
            return true;
        }


        private bool UpdateSection(Label label, MetroProgressSpinner spinner, Func<bool> method)
        {
            label.ForeColor = System.Drawing.Color.Black;

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
            if(!logicClick)
                e.Cancel = true;
            logicClick = false;
        }

        private void AgreeBtnOnClick(object sender, EventArgs eventArgs)
        {
            logicClick = true;
            tabControl.SelectTab(tabControl.SelectedIndex + 1);
        }

        private void InstallBtnOnClick(object sender, EventArgs eventArgs)
        {
            logicClick = true;
            tabControl.SelectTab(tabControl.SelectedIndex + 1);
            installThread.Start();
        }

        private void FinishBtnOnClick(object sender, EventArgs eventArgs)
        {
            success = true;
            Application.Exit();
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void beginClick(object sender, EventArgs e)
        {
            logicClick = true;
            tabControl.SelectTab(tabControl.SelectedIndex + 1);
        }

        private void UpdateSpinner()
        {
            while (true)
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
