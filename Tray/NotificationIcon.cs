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
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FOG.Handlers;

namespace FOG
{
    public sealed class NotificationIcon
    {
        //Define variables
        private readonly NotifyIcon _notifyIcon;
        private Notification _notification;

        #region Main - Program entry point

        /// <summary>Program entry point.</summary>
        /// <param name="args">Command Line Arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isFirstInstance;
            // Please use a unique name for the mutex to prevent conflicts with other programs
            using (new Mutex(true, "FOG-TRAY", out isFirstInstance))
            {
                if (!isFirstInstance) return;
                var notificationIcon = new NotificationIcon();
                notificationIcon._notifyIcon.Visible = true;
                Application.Run();
                notificationIcon._notifyIcon.Dispose();
            } // releases the Mutex
        }

        #endregion

        #region Event Handlers

        private void IconDoubleClick(object sender, EventArgs e)
        {
        }

        #endregion

        #region Initialize icon and menu

        public NotificationIcon()
        {
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Notification, OnNotification);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);

            _notifyIcon = new NotifyIcon();
            var notificationMenu = new ContextMenu(InitializeMenu());

            _notifyIcon.DoubleClick += IconDoubleClick;
            var resources = new ComponentResourceManager(typeof (NotificationIcon));
            _notifyIcon.Icon = (Icon) resources.GetObject("icon");
            _notifyIcon.ContextMenu = notificationMenu;

            _notification = new Notification();
        }

        private static void OnUpdate(dynamic data)
        {
            if (data.action.Equals("start"))
                Application.Exit();
        }

        //Called when a message is recieved from the bus
        private void OnNotification(dynamic data)
        {
            try
            {
                _notification.Title = data.notification;
            }
            catch (Exception ex)
            {
                return;
            }


            _notifyIcon.BalloonTipTitle = _notification.Title;
            _notifyIcon.BalloonTipText = _notification.Message;
            _notifyIcon.ShowBalloonTip(_notification.Duration);
            _notification = new Notification();
        }

        private static MenuItem[] InitializeMenu()
        {
            var menu = new MenuItem[] {};
            return menu;
        }

        #endregion
    }
}