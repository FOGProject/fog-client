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
using Newtonsoft.Json.Linq;

namespace FOG
{
    internal class WindowsInstall : IInstall
    {
        private static string jsonFile = Path.Combine(
                Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine),
                "session.json");

        private string tmpLocation = Path.GetTempPath();

        public bool PrepareFiles()
        {
            return Helper.ExtractResource("FOG.Scripts.FOGService.msi", Path.Combine(tmpLocation, "FOGService.msi"));
        }

        public bool Install()
        {
            var process = new Process
            {
                StartInfo =
                {
                    Arguments = $"/i \"{Path.Combine(tmpLocation, "FOGService.msi")}"
                }
            };
            process.StartInfo.FileName = "msiexec";

            process.Start();

            return process.ExitCode == 0;
        }

        public bool Configure()
        {
            return true;
        }

        public string GetLocation()
        {
            var config = GetSettings();
            return GetValue("INSTALLDIR", config);
        }

        public bool Uninstall()
        {
            // TODO: Obtain product codes of installed version(s)
            //return ProcessHandler.Run("msiexec.exe", "/x " + ProductCode) == 0;
            throw new NotImplementedException();
        }

        private static JObject GetSettings()
        {
            return JObject.Parse(File.ReadAllText(jsonFile));
        }

        private static string GetValue(string key, JObject config)
        {
            var value = config.GetValue(key);
            return string.IsNullOrEmpty(value.ToString().Trim()) ? string.Empty : value.ToString().Trim();
        }
    }
}