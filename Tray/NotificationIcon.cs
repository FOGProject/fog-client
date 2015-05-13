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
        private bool _isNotificationReady;
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
            // Setup the pipe client


            var userNotificationPipe = new PipeClient("fog_pipe_notification_user_" + UserHandler.GetCurrentUser());
            userNotificationPipe.MessageReceived += pipeNotificationClient_MessageReceived;
            userNotificationPipe.Connect();

            var systemNotificationPipe = new PipeClient("fog_pipe_notification");
            systemNotificationPipe.MessageReceived += pipeNotificationClient_MessageReceived;
            systemNotificationPipe.Connect();

            var servicePipe = new PipeClient("fog_pipe_service");
            servicePipe.MessageReceived += pipeNotificationClient_MessageReceived;
            servicePipe.Connect();


            _notifyIcon = new NotifyIcon();
            var notificationMenu = new ContextMenu(InitializeMenu());

            _notifyIcon.DoubleClick += IconDoubleClick;
            var resources = new ComponentResourceManager(typeof (NotificationIcon));
            _notifyIcon.Icon = (Icon) resources.GetObject("icon");
            _notifyIcon.ContextMenu = notificationMenu;

            _notification = new Notification();
            _isNotificationReady = false;
        }

        //Called when a message is recieved from the pipe server
        private void pipeNotificationClient_MessageReceived(string message)
        {
            if (message.StartsWith("TLE:"))
            {
                message = message.Substring(4);
                _notification.Title = message;
            }
            else if (message.StartsWith("MSG:"))
            {
                message = message.Substring(4);
                _notification.Message = message;
            }
            else if (message.StartsWith("DUR:"))
            {
                message = message.Substring(4);
                try
                {
                    _notification.Duration = int.Parse(message);
                }
                catch
                {
                    // ignored
                }
                _isNotificationReady = true;
            }
            else if (message.Equals("UPD"))
            {
                Application.Exit();
            }

            if (_isNotificationReady)
            {
                _notifyIcon.BalloonTipTitle = _notification.Title;
                _notifyIcon.BalloonTipText = _notification.Message;
                _notifyIcon.ShowBalloonTip(_notification.Duration);
                _isNotificationReady = false;
                _notification = new Notification();
            }
        }

        private MenuItem[] InitializeMenu()
        {
            var menu = new MenuItem[] {};
            return menu;
        }

        #endregion
    }
}