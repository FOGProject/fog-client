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
using System.Windows.Forms;
using UserNotification;
using Zazzles;

namespace FOG
{
    class NotificationInvoker : IDisposable
    {
        private NotificationGUI notification;
        private Form context;

        public NotificationInvoker(Form context)
        {
            this.context = context;
            notification = new NotificationGUI();
        }

        public void UpdateText(string title, string body)
        {
            context.Invoke(new MethodInvoker(delegate
            {
                notification.SetTitle(title);
                notification.SetBody(body);
            }));
        }

        public void Show()
        {
            context.Invoke(new MethodInvoker(delegate
                {
                    notification.StartFade();
                }));
        }

        public void UpdatePosition(int index)
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            var height = workingArea.Bottom - notification.Height;
            if (Settings.OS == Settings.OSType.Mac) height = height - 22;

            height = (Settings.OS == Settings.OSType.Windows)
                ? height - (notification.Height + 5) * index
                : height + (notification.Height + 5) * index;

            try
            {
                notification.Invoke(new MethodInvoker(
                    delegate { notification.Location = new Point(workingArea.Right - notification.Width, height); }));
            }
            catch (Exception) { }

            try
            {
                notification.Location = new Point(workingArea.Right - notification.Width, height);
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            //notification.Dispose();
        }
    }
}
