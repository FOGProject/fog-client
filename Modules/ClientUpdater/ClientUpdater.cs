﻿/*
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
using FOG.Handlers.Data;
using FOG.Handlers.Middleware;
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
            var serverVersion = Communication.GetRawResponse("/service/getversion.php?client");
            var localVersion = RegistryHandler.GetSystemSetting("Version");
            try
            {
                var server = int.Parse(serverVersion.Replace(".", ""));
                var local = int.Parse(localVersion.Replace(".", ""));

                if (server <= local) return;

                var updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", "FOGService.msi");

                if (File.Exists(updaterPath))
                    File.Delete(updaterPath);

                Communication.DownloadFile("/client/FOGService.msi", updaterPath);

                if (!IsAuthenticate(updaterPath)) return;

                PrepareUpdateHelpers();
                Power.Updating = true;
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Unable to parse versions");
                Log.Error(Name, ex);
            }
        }

        //Prepare the downloaded update
        private void PrepareUpdateHelpers()
        {

            if (!File.Exists(string.Format("{0}\\FOGUpdateHelper.exe", AppDomain.CurrentDomain.BaseDirectory)) &&
                !File.Exists(string.Format("{0}\\FOGUpdateWaiter.exe", AppDomain.CurrentDomain.BaseDirectory)))
            {
                Log.Error(Name, "Unable to locate helper files");
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

                File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\Newtonsoft.Json.dll",
                    AppDomain.CurrentDomain.BaseDirectory + @"tmp\Newtonsoft.Json.dll", true);
                
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Unable to prepare update helpers");
                Log.Error(Name, ex);
            }
        }

        private bool IsAuthenticate(string filePath)
        {
            var signeeCert = RSA.ExtractDigitalSignature(filePath);
            var targetSigner = RSA.FOGProjectCertificate();
            if (RSA.IsFromCA(targetSigner, signeeCert))
            {
                Log.Entry(Name, "Update file is authentic");
                return true;
            }

            Log.Error(Name, "Update file is not authentic");
            return false;
        }
    }
}