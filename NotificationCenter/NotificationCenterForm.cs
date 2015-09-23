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
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FOG.Core;

namespace NotificationCenter
{
    public partial class NotificationCenterForm : Form
    {
        public NotificationCenterForm()
        {
            bool isFirstInstance;
            using (new Mutex(true, "FOG-TRAY", out isFirstInstance))
            {
                if (!isFirstInstance)
                    Application.Exit();
            }

            InitializeComponent();
            var workingArea = Screen.GetWorkingArea(this);
            var height = workingArea.Bottom - Size.Height;
            if (Settings.OS == Settings.OSType.Mac) height = height - 22;

            Location = new Point(workingArea.Right - Size.Width, height);

            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Notification, OnNotification);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoadLog()
        {
            var logPath = Path.Combine(Settings.Location, "logs", DateTime.Today.ToString("yy-MM-dd"));
            using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                eventList.Items.Clear();
                eventList.Items.AddRange(sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
            }
        }

        private void OnNotification(dynamic data)
        {
            LoadLog();
        }
    }
}
