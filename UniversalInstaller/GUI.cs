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
        private Color defaultSpinnerColor;

        public GUI()
        {
            InitializeComponent();
            defaultSpinnerColor = progressBar1.ForeColor;
            UpdateWelcomeText();

            serverUpThread = new Thread(UpdateSpinner)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "server-status"
            };
            serverUpThread.Start();
        }

        public void UpdateWelcomeText()
        {
            this.welcomeText.Text = this.welcomeText.Text.Replace("{OPERATING_SYSTEM}", Settings.OS.ToString());
        }

        private void exitButton_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void ServerChanged(object sender, System.EventArgs e)
        {
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
        }

        private void FinishBtnOnClick(object sender, EventArgs eventArgs)
        {
            success = true;
            Application.Exit();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            logicClick = true;
            tabControl.SelectTab(tabControl.SelectedIndex + 1);
        }

        private void UpdateSpinner()
        {
            while (true)
            {
                var serverStatus = CheckServer();

                progressBar1.Invoke((MethodInvoker)(() =>
                {
                    progressBar1.ForeColor = (serverStatus) ? Color.ForestGreen : Color.Crimson;
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
                    //url += "/service/getversion.php";

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
