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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Zazzles;

namespace FOG
{
    public sealed class NotificationIcon
    {
        private readonly NotifyIcon _notifyIcon;
        private const int Duration =  10 * 1000;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isFirstInstance;
            using (new Mutex(true, "FOG-TRAY", out isFirstInstance))
            {
                if (!isFirstInstance) return;
                var notificationIcon = new NotificationIcon();
                notificationIcon._notifyIcon.Visible = true;
                Application.Run();
                notificationIcon._notifyIcon.Dispose();
            }
        }

        private void IconDoubleClick(object sender, EventArgs e)
        {
        }

        public NotificationIcon()
        {
            Log.Output = Log.Mode.Quiet;
            Eager.Initalize();
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Notification, OnNotification);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);
            Bus.Subscribe(Bus.Channel.Status, OnStatus);

            _notifyIcon = new NotifyIcon();
            var notificationMenu = new ContextMenu(InitializeMenu());

            _notifyIcon.DoubleClick += IconDoubleClick;
            _notifyIcon.Icon = new Icon(Path.Combine(Settings.Location, "logo.ico"));
            _notifyIcon.ContextMenu = notificationMenu;
            _notifyIcon.Text = "FOG Client v" + Settings.Get("Version");
        }

        private static void OnUpdate(dynamic data)
        {
            if (data.action == null) return;

            if (data.action.Equals("start"))
                Application.Exit();
        }

        //Called when a message is recieved from the bus
        private void OnNotification(dynamic data)
        {
            if (data.title == null || data.message == null) return;
            try
            {
                _notifyIcon.BalloonTipTitle = data.title;
                _notifyIcon.BalloonTipText = data.message;
                _notifyIcon.ShowBalloonTip(Duration);
            }
            catch (Exception)
            {
                return;
            }
        }

        private void OnStatus(dynamic data)
        {
            if (data.action == null) return;
            if (data.action.toString().Equals("unload"))
                Application.Exit();
        }

        private static MenuItem[] InitializeMenu()
        {
            var menu = new MenuItem[] { };
            return menu;
        }
     }
}