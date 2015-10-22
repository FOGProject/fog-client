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
using Gdk;
using Gtk;
using Zazzles;
using Application = Gtk.Application;

namespace FOG.Tray.GTK
{
    public sealed class GTKTray : ITray
    {
        private StatusIcon _notifyIcon;

        public GTKTray(string icon)
        {
            Application.Init();
            _notifyIcon = new StatusIcon(new Pixbuf(icon))
            {
                Visible = true
            };
            _notifyIcon.Activate += NotifyIconOnActivate;
            Application.Run();

        }

        private void NotifyIconOnActivate(object sender, EventArgs eventArgs)
        {
            
            Notification.Emit(Notification.ToJSON("Test", "You clicked", "55"), false, false);
        }

        public void SetHover(string text)
        {
            _notifyIcon.Tooltip = text;
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }
    }
}