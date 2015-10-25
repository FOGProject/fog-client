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
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using FOG.Tray.GTK;
using Zazzles;
using UserNotification;
namespace FOG.Tray
{
    public sealed class Tray
    {
        private static List<NotificationInvoker> _notifications = new List<NotificationInvoker>();
        private static Form contextForm;
        private static Thread trayThread;
        private static object locker = new object();

        [STAThread]
        public static void Main(string[] args)
        {
            Log.Output = Log.Mode.Console;

            contextForm = new Form();
            contextForm.Size = new Size(0, 0);
            contextForm.Opacity = 0;
            contextForm.Enabled = false;
            contextForm.ShowInTaskbar = false;

            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Notification, OnNotification);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);

            trayThread = new Thread(() => ShowTray());
            trayThread.Start();

            contextForm.Show();
            Application.Run();
        }

        private static void ShowTray()
        {
            ITray _instance;
            switch (Settings.OS)
            {
                default:
                    _instance = new GTKTray(Path.Combine(Settings.Location, "logo.ico"));
                    break;
            }
            _instance.SetHover("FOG Client v" + Settings.Get("Version"));
        }

        private static void OnUpdate(dynamic data)
        {
            if (data.action == null) return;

            if (data.action.Equals("start"))
            {
                Application.Exit();
                Environment.Exit(0);
            }
        }

        private static void ReOrderNotifications()
        {
            for (var i = 0; i < _notifications.Count; i++)
            {
                _notifications[i].UpdatePosition(i);
            }
        }

        private static void OnNotification(dynamic data)
        {
            if (data.title == null || data.message == null) return;
            var invoker = new NotificationInvoker(contextForm);
            invoker.UpdateText(data.title.ToString(), data.message.ToString());

            lock (locker)
            {
                _notifications.Add(invoker);
                invoker.UpdatePosition(_notifications.IndexOf(invoker));
            }

            invoker.GUI.Disposed += delegate
            {
                lock (locker)
                {
                    _notifications.Remove(invoker);
                    ReOrderNotifications();
                }
            };
            invoker.Show();
        }
    }
}
