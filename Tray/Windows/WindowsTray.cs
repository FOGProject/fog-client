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

using System.Drawing;
using System.Windows.Forms;

namespace FOG.Tray
{
    public sealed class WindowsTray : ITray
    {
        private readonly NotifyIcon _notifyIcon;

        public WindowsTray(string iconPath, string hoverText)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _notifyIcon = new NotifyIcon();
            var notificationMenu = new ContextMenu(InitializeMenu());

            _notifyIcon.Icon = new Icon(iconPath);
            _notifyIcon.ContextMenu = notificationMenu;
            _notifyIcon.Text = hoverText;
            _notifyIcon.Visible = true;
            Application.Run();
        }

        public NotifyIcon GetForm()
        {
            return this._notifyIcon; 
        }

        private static MenuItem[] InitializeMenu()
        {
            var menu = new MenuItem[] { };
            return menu;
        }

        public void Notification(string title, string body, int duration)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = body;
            _notifyIcon.ShowBalloonTip(duration);
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }
    }
}