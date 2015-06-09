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
using System.IO;
using System.Reflection;
using System.Threading;
using FOG.Handlers;

namespace FOG
{
    /// <summary>
    ///     Description of Updater.
    /// </summary>
    public static class UpdateHandler
    {
        private const string LogName = "Service-Update";

        private static void KillSubProcesses()
        {
            //If the User Service is still running, wait 120 seconds and kill it

            while (Process.GetProcessesByName("FOGUserService").Length > 0)
            {
                Thread.Sleep(12*1000);
                foreach (var process in Process.GetProcessesByName("FOGUserService"))
                {
                    process.Kill();
                }
            }
        }

        public static void BeginUpdate()
        {
            try
            {
                //Create updating.info which will warn any sub-processes currently initializing that they should halt
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\tmp\updating.info", "");

                //Give time for any sub-processes that may be in the middle of initializing and missed the updating.info file so they can recieve the update pipe notice
                Thread.Sleep(1000);

                //Notify all FOG sub processes that an update is about to occu
                Bus.Emit(Bus.Channel.Update, "begin", true);

                //Kill any FOG sub processes still running after the notification
                KillSubProcesses();

                //Launch the updater
                Log.Entry(LogName, "Spawning update helper");

                var process = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                   @"\tmp\FOGUpdateHelper.exe"
                    }
                };
                process.Start();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to perform update");
                Log.Error(LogName, ex);
            }
        }
    }
}