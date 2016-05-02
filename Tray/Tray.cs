/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2016 FOG Project
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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Zazzles;

namespace FOG.Tray
{
    public sealed class Tray
    {
        private static ITray _instance;
        //private static Thread _trayThread;

        /// <summary>Program entry point.</summary>
        /// <param name="args">Command Line Arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            Log.Output = Log.Mode.Quiet;

            bool isFirstInstance;
            using (new Mutex(true, "FOG-TRAY", out isFirstInstance))
            {
                if (!isFirstInstance) return;
            }

            Eager.Initalize();
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Notification, OnNotification);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);

            //_trayThread = new Thread(ShowTray);
            //_trayThread.Start();
            ShowTray();
        }

        private static void ShowTray()
        {
            _instance = new WindowsTray(Path.Combine(Settings.Location, "logo.ico"), "FOG Client v" + Settings.Get("Version"));
        }
      
        private static void OnUpdate(dynamic data)
        {
            if (data.action == null) return;

            if (data.action.Equals("start"))
                Application.Exit();
        }

        //Called when a message is recieved from the bus
        private static void OnNotification(dynamic data)
        {
            if (data.title == null || data.message == null)
                return;

            _instance.Notification(data.title, data.message, 10*1000);

            //SpawnGUIThread(data.title.ToString(), data.message.ToString());
        }

        private static MenuItem[] InitializeMenu()
        {
            var menu = new MenuItem[] { };
            return menu;
        }
    }
}