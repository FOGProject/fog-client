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
    class NotificationInvoker
    {
        public NotificationGUI GUI {get; }
        private Form context;

        public NotificationInvoker(Form context)
        {
            this.context = context;
            GUI = new NotificationGUI();
        }

        public void UpdateText(string title, string body)
        {
            context.Invoke(new MethodInvoker(delegate
            {
                GUI.SetTitle(title);
                GUI.SetBody(body);
            }));
        }

        public void Show()
        {
            context.Invoke(new MethodInvoker(delegate
                {
                    GUI.StartFade();
                }));
        }

        public void UpdatePosition(int index)
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            var height = workingArea.Bottom - GUI.Height;

            height = (Settings.OS == Settings.OSType.Windows)
                ? height - (GUI.Height + 5) * index
                :  (GUI.Height + 5) * index;

            if (Settings.OS != Settings.OSType.Windows) height += 22;

            try
            {
                GUI.Invoke(new MethodInvoker(
                    delegate { GUI.Location = new Point(workingArea.Right - GUI.Width, height); }));
            }
            catch (Exception) { }

            try
            {
                GUI.Location = new Point(workingArea.Right - GUI.Width, height);
            }
            catch (Exception) { }
        }
    }
}
