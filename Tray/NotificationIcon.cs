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
using System.Threading;
using System.Windows.Forms;

namespace FOG
{
    public sealed class NotificationIcon
    {
		
        //Define variables
        private NotifyIcon notifyIcon;
        private ContextMenu notificationMenu;
        private PipeClient systemNotificationPipe;
        private PipeClient userNotificationPipe;
        private PipeClient servicePipe;
		
        private Notification notification;
        private Boolean isNotificationReady;
		
        #region Initialize icon and menu
        public NotificationIcon()
        {
			
            // Setup the pipe client

			
            this.userNotificationPipe = new PipeClient("fog_pipe_notification_user_" + UserHandler.GetCurrentUser());
            this.userNotificationPipe.MessageReceived += new PipeClient.MessageReceivedHandler(pipeNotificationClient_MessageReceived);
            this.userNotificationPipe.connect();				
			
            this.systemNotificationPipe = new PipeClient("fog_pipe_notification");
            this.systemNotificationPipe.MessageReceived += new PipeClient.MessageReceivedHandler(pipeNotificationClient_MessageReceived);
            this.systemNotificationPipe.connect();	
			
            this.servicePipe = new PipeClient("fog_pipe_service");
            this.servicePipe.MessageReceived += new PipeClient.MessageReceivedHandler(pipeNotificationClient_MessageReceived);
            this.servicePipe.connect();				
			
			
			
            notifyIcon = new NotifyIcon();
            notificationMenu = new ContextMenu(InitializeMenu());
			
            notifyIcon.DoubleClick += IconDoubleClick;
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
            notifyIcon.Icon = (Icon)resources.GetObject("icon");
            notifyIcon.ContextMenu = notificationMenu;
			
            this.notification = new Notification();
            this.isNotificationReady = false;
        }
		
        //Called when a message is recieved from the pipe server
        private void pipeNotificationClient_MessageReceived(String message)
        {
			
            if (message.StartsWith("TLE:"))
            {
                message = message.Substring(4);
                this.notification.Title = message;
            }
            else if (message.StartsWith("MSG:"))
            {
                message = message.Substring(4);
                this.notification.Message = message;
            }
            else if (message.StartsWith("DUR:"))
            {
                message = message.Substring(4);
                try
                {
                    this.notification.Duration = int.Parse(message);
                }
                catch
                {
                }
                this.isNotificationReady = true;
            }
            else if (message.Equals("UPD"))
            {
                Application.Exit();
            }
			
            if (this.isNotificationReady)
            {
                this.notifyIcon.BalloonTipTitle = this.notification.Title;
                this.notifyIcon.BalloonTipText = this.notification.Message;
                this.notifyIcon.ShowBalloonTip(this.notification.Duration);
                this.isNotificationReady = false;
                this.notification = new Notification();
            }
        }
		
        private MenuItem[] InitializeMenu()
        {
            var menu = new MenuItem[] { };
            return menu;
        }
        #endregion
		
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
            using (var mtx = new Mutex(true, "Tray", out isFirstInstance))
            {
                if (isFirstInstance)
                {
                    var notificationIcon = new NotificationIcon();
                    notificationIcon.notifyIcon.Visible = true;
                    Application.Run();
                    notificationIcon.notifyIcon.Dispose();
                }
                else
                {
                    // The application is already running
                    // TODO: Display message box or change focus to existing application instance
                }
            } // releases the Mutex
        }
        #endregion
		
        #region Event Handlers

        private void IconDoubleClick(object sender, EventArgs e)
        {
        }
        #endregion
    }
}
