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
using System.IO;
using FOG.Handlers;

namespace FOG.Modules
{
    /// <summary>
    ///     Update the FOG Service
    /// </summary>
    public class ClientUpdater : AbstractModule
    {
        public ClientUpdater()
        {
            Name = "ClientUpdater";
            Description = "Update the FOG Service";
        }

        protected override void doWork()
        {
            var serverVersion = CommunicationHandler.GetRawResponse("/service/getversion.php?clientver");
            var localVersion = RegistryHandler.GetSystemSetting("Version");
            try
            {
                var server = int.Parse(serverVersion.Replace(".", ""));
                var local = int.Parse(localVersion.Replace(".", ""));

                if (server > local)
                {
                    CommunicationHandler.DownloadFile("/client/FOGService.msi",
                        AppDomain.CurrentDomain.BaseDirectory + @"\tmp\FOGService.msi");
                    prepareUpdateHelpers();
                    ShutdownHandler.UpdatePending = true;
                }
            }
            catch (Exception ex)
            {
                LogHandler.Log(Name, "Unable to parse versions");
                LogHandler.Log(Name, "ERROR: " + ex.Message);
            }
        }

        //Prepare the downloaded update
        private void prepareUpdateHelpers()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateHelper.exe") &&
                File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateWaiter.exe"))
            {
                try
                {
                    File.Move(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateHelper.exe",
                        AppDomain.CurrentDomain.BaseDirectory + @"tmp\FOGUpdateHelper.exe");
                    File.Move(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateWaiter.exe",
                        AppDomain.CurrentDomain.BaseDirectory + @"tmp\FOGUpdateWaiter.exe");
                    File.Move(AppDomain.CurrentDomain.BaseDirectory + @"\Handlers.dll",
                        AppDomain.CurrentDomain.BaseDirectory + @"tmp\Handlers.dll");
                }
                catch (Exception ex)
                {
                    LogHandler.Log(Name, "Unable to prepare update helpers");
                    LogHandler.Log(Name, "ERROR: " + ex.Message);
                }
            }
            else
            {
                LogHandler.Log(Name, "Unable to locate helper files");
            }
        }
    }
}