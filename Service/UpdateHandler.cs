/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
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
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using Zazzles;

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
            try
            {
                UserServiceSpawner.KillAll();
                Thread.Sleep(5*1000);
                ProcessHandler.KillAllEXE("FOGUserService");
                ProcessHandler.KillAllEXE("FOGTray");
                Thread.Sleep(5*1000);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not stop sub processes");
                Log.Error(LogName, ex);
            }
        }

        public static void BeginUpdate()
        {
            try
            {
                UserServiceSpawner.Stop();

                //Create updating.info which will warn any sub-processes currently initializing that they should halt
                File.WriteAllText(Path.Combine(Settings.Location, "updating.info"), "foobar");

                //Give time for any sub-processes that may be in the middle of initializing and missed the updating.info file so they can recieve the update pipe notice
                Thread.Sleep(1000);

                //Notify all FOG sub processes that an update is about to occur
                dynamic json = new JObject();
                json.action = "start";

                Bus.Emit(Bus.Channel.Update, json, true);

                //Kill any FOG sub processes still running after the notification
                KillSubProcesses();

                //Launch the updater
                Log.Entry(LogName, "Spawning update helper");

                ProcessHandler.RunClientEXE(Path.Combine("tmp", "FOGUpdateHelper.exe"), $"\"{Log.FilePath}\"", false);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to perform update");
                Log.Error(LogName, ex);
            }
        }
    }
}