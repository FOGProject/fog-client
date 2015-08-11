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
using System.IO;
using FOG.Handlers;
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
            Compatiblity = Settings.OSType.Windows;
        }

        protected override void DoWork()
        {
            var serverVersion = Communication.GetRawResponse("/service/getversion.php?client");
            var localVersion = Settings.Get("Version");
            try
            {
                var server = int.Parse(serverVersion.Replace(".", ""));
                var local = int.Parse(localVersion.Replace(".", ""));

                if (server <= local) return;

                var updater = (Settings.OS == Settings.OSType.Windows)
                    ? "FOGService.msi"
                    : "core.sh";

                if (File.Exists(Path.Combine(Settings.Location, "tmp", updater)))
                    File.Delete(Path.Combine(Settings.Location, "tmp", updater));

                Communication.DownloadFile("/client/" + updater,
                    Path.Combine(Settings.Location, "tmp", updater));

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
            var files = new List<string>
            {
                "FOGUpdateHelper.exe",
                "FOGUpdateWaiter.exe",
                "Handlers.dll",
                "Newtonsoft.Json.dll",
                "settings.json",
                "token.dat"
            };

            foreach (var file in files)
            {
                try
                {
                    File.Copy(Path.Combine(Settings.Location, file),
                        Path.Combine(Settings.Location, "tmp", file), true);
                }
                catch (Exception ex)
                {
                    Log.Error(Name, "Unable to prepare file:" + file);
                    Log.Error(Name, ex);
                }
            }
        }
    }
}