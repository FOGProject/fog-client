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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using FOG.Tray.GTK;
using Zazzles;

namespace FOG.Tray
{
    public sealed class Tray
    {
        private static List<NotificationInvoker> _notifications = new List<NotificationInvoker>();
        private static Form contextForm;

        private static ITray _instance;

        /// <summary>Program entry point.</summary>
        /// <param name="args">Command Line Arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            Log.Output = Log.Mode.Console;

            contextForm = new Form();
            contextForm.Show();
            contextForm.Hide();

            bool isFirstInstance;
            using (new Mutex(true, "FOG-TRAY", out isFirstInstance))
            {
                if (!isFirstInstance) return;
            }

            Eager.Initalize();
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Notification, OnNotification);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);

            var hoverText = "FOG Client v" + Settings.Get("Version");

            switch (Settings.OS)
            {
                default:
                    _instance = new GTKTray(Path.Combine(Settings.Location,"logo.ico"));
                    break;
            }
            _instance.SetHover(hoverText);
        }

        private static void OnUpdate(dynamic data)
        {
            if (data.action == null) return;

            if (data.action.Equals("start"))
                Application.Exit();
        }

        private static void OnNotification(dynamic data)
        {
            if (data.title == null || data.message == null) return;
            var invoker = new NotificationInvoker(contextForm);
            invoker.UpdateText(data.title.ToString(), data.message.ToString());
            _notifications.Add(invoker);
            invoker.UpdatePosition(_notifications.IndexOf(invoker));
            invoker.Show();
        }
    }
}