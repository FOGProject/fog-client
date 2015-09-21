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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FOG.Core.User
{
    internal class LinuxUser : IUser
    {
        private const string LogName = "UserHandler";

        public List<string> GetUsersLoggedIn()
        {
            var usersInfo = new List<string>();

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "w",
                    Arguments = "-h -s",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                process.Start();
                while (!process.StandardOutput.EndOfStream)
                    usersInfo.Add(process.StandardOutput.ReadLine());
            }

            return usersInfo.Select(userInfo => userInfo.Substring(0, userInfo.IndexOf(" "))).ToList();
        }

        public int GetInactivityTime()
        {
            var time = "-1";

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xprintidle",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                process.Start();
                while (!process.StandardOutput.EndOfStream)
                    time = process.StandardOutput.ReadLine();
            }

            try
            {
                if (time != null) return int.Parse(time);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Can not detect idle time");
                Log.Error(LogName, ex);
            }
            return -1;
        }
    }
}