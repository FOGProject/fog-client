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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FOG.Handlers.Power
{
    class WindowsPower : IPower
    {
        //Load the ability to lock the computer from the native user32 dll
        [DllImport("user32")]
        private static extern void lockWorkStation();

        public void Shutdown(string comment, Power.FormOption options = Power.FormOption.Abort, string message = null, int seconds = 30)
        {
            Power.QueueShutdown(string.Format("/s /c \"{0}\" /t {1}", comment, seconds), options, message);
        }

        public void Restart(string comment, Power.FormOption options = Power.FormOption.Abort, string message = null, int seconds = 30)
        {
            Power.QueueShutdown(string.Format("/r /c \"{0}\" /t {1}", comment, seconds), options, message);
        }

        public void LogOffUser()
        {
            Power.CreateTask("/l");
        }

        public void Hibernate()
        {
            Power.CreateTask("/h");
        }

        public void LockWorkStation()
        {
            lockWorkStation();
        }

        public void CreateTask(string parameters)
        {
            Process.Start("shutdown", parameters);
        }
    }
}
