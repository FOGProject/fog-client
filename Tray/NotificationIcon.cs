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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FOG.Core;
using UserNotification;

namespace FOG
{
    public sealed class NotificationIcon
    {
        //Define variables
        private readonly NotifyIcon _notifyIcon;
        private static volatile List<NotificationGUI> _notifications = new List<NotificationGUI>();
        #region Main - Program entry point

        /// <summary>Program entry point.</summary>
        /// <param name="args">Command Line Arguments</param>
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
            };
            Application.Run();
        }

        #endregion


        #region Initialize icon and menu

        public NotificationIcon()
        {
            Eager.Initalize();
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Notification, OnNotification);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);

            _notifyIcon = new NotifyIcon();
            var notificationMenu = new ContextMenu(InitializeMenu());

            var resources = new ComponentResourceManager(typeof (NotificationIcon));
            _notifyIcon.Icon = (Icon) resources.GetObject("icon");
            _notifyIcon.ContextMenu = notificationMenu;
            _notifyIcon.Text = "FOG Client v" + Settings.Get("Version");
            _notifyIcon.DoubleClick += OnClick;
        }

        private static void UpdateFormLocation(int index)
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            var height = workingArea.Bottom - _notifications[index].Height;
            if (Settings.OS == Settings.OSType.Mac) height = height - 22;

            height = (Settings.OS == Settings.OSType.Windows)
                ? height - (_notifications[index].Height+5)*index
                : height + (_notifications[index].Height+5)*index;

            try
            {
                _notifications[index].Invoke(new MethodInvoker(
                    delegate { _notifications[index].Location = new Point(workingArea.Right - _notifications[index].Width, height); }));
            }
            catch (Exception) { }

            try
            {
                _notifications[index].Location = new Point(workingArea.Right - _notifications[index].Width, height);
            }
            catch (Exception) { }
        }

        private void OnClick(object sender, EventArgs e)
        {
            SpawnGUIThread("Test Title", "Test Body");
        }

        private static void SpawnGUIThread(string title, string body)
        {
            var notThread = new Thread(() => SpawnForm(title, body))
            {
                Priority = ThreadPriority.Normal,
                IsBackground = false,
            };
            notThread.Start();
        }

        private static void SpawnForm(string title, string body)
        {
            var notForm = new NotificationGUI(title, body);
            _notifications.Add(notForm);
            notForm.Disposed += delegate
            {
                _notifications.Remove(notForm);
                ReOrderNotifications();
                notForm.Dispose();
                notForm = null;
            };
            UpdateFormLocation(_notifications.Count-1);
            Application.Run(notForm);
        }

        private static void ReOrderNotifications()
        {
            for(var i=0; i < _notifications.Count; i++ )
            {
                UpdateFormLocation(i);
            }
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
            SpawnGUIThread(data.title.ToString(), data.message.ToString());
        }

        private static MenuItem[] InitializeMenu()
        {
            var menu = new MenuItem[] {};
            return menu;
        }

        #endregion
    }
}