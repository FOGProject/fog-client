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
using FOG.Handlers.Power;


namespace FOG.Modules.ClientUpdater
{
    /// <summary>
    ///     Update the FOG Service
    /// </summary>
    public class ClientUpdater : AbstractModule
    {
        public ClientUpdater()
        {
            Name = "ClientUpdater";
        }

        protected override void DoWork()
        {
            var serverVersion = Middleware.GetRawResponse("/service/getversion.php?client");
            var localVersion = RegistryHandler.GetSystemSetting("Version");
            try
            {
                var server = int.Parse(serverVersion.Replace(".", ""));
                var local = int.Parse(localVersion.Replace(".", ""));

                if (server <= local) return;

                if (File.Exists(string.Format("{0}\\tmp\\FOGService.msi", AppDomain.CurrentDomain.BaseDirectory)))
                    File.Delete(string.Format("{0}\\tmp\\FOGService.msi", AppDomain.CurrentDomain.BaseDirectory));     

                Middleware.DownloadFile("/client/FOGService.msi",
                    AppDomain.CurrentDomain.BaseDirectory + @"\tmp\FOGService.msi");

                PrepareUpdateHelpers();
                Power.UpdatePending = true;
            }
            catch (Exception ex)
            {
                LogHandler.Error(Name, "Unable to parse versions");
                LogHandler.Error(Name, ex);
            }
        }

        //Prepare the downloaded update
        private void PrepareUpdateHelpers()
        {

            if (!File.Exists(string.Format("{0}\\FOGUpdateHelper.exe", AppDomain.CurrentDomain.BaseDirectory)) &&
                !File.Exists(string.Format("{0}\\FOGUpdateWaiter.exe", AppDomain.CurrentDomain.BaseDirectory)))
            {
                LogHandler.Error(Name, "Unable to locate helper files");
                return;
            }
            
            try
            {                         
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateHelper.exe",
                    AppDomain.CurrentDomain.BaseDirectory + @"tmp\FOGUpdateHelper.exe", true);

                File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\FOGUpdateWaiter.exe",
                    AppDomain.CurrentDomain.BaseDirectory + @"tmp\FOGUpdateWaiter.exe", true);

                File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\Handlers.dll",
                    AppDomain.CurrentDomain.BaseDirectory + @"tmp\Handlers.dll", true);
            }
            catch (Exception ex)
            {
                LogHandler.Error(Name, "Unable to prepare update helpers");
                LogHandler.Error(Name, ex);
            }
        }
    }
}