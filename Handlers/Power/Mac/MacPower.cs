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
using System.Diagnostics;

namespace FOG.Handlers.Power
{
    class MacPower : IPower
    {
        public void Shutdown(string comment, Power.FormOption options = Power.FormOption.Abort, string message = null, int seconds = 30)
        {
            var minutes = seconds / 60.0;
            var timeDelay = (int)Math.Round(minutes);

            Power.QueueShutdown(string.Format("-h +{0} \"{1}\"", timeDelay, comment), options, message);
        }

        public void Restart(string comment, Power.FormOption options = Power.FormOption.Abort, string message = null, int seconds = 30)
        {
            var minutes = seconds / 60.0;
            var timeDelay = (int)Math.Round(minutes);

            Power.QueueShutdown(string.Format("-r +{0} \"{1}\"", timeDelay, comment), options, message);
        }

        public void LogOffUser()
        {
            Process.Start("logout");
        }

        public void Hibernate()
        {
            Process.Start("pmset -a hibernatemode 25");
        }

        public void LockWorkStation()
        {
            Process.Start(@"/System/Library/CoreServices/Menu\ Extras/User.menu/Contents/Resources/CGSession -suspend");
        }

        public void CreateTask(string parameters)
        {
            Process.Start("shutdown", parameters);
        }
    }
}
