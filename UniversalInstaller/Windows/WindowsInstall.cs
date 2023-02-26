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
using System.Diagnostics;
using System.IO;

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
                    Arguments = $"/i \"{Path.Combine(tmpLocation, "FOGService.msi")}\""
                }
            };
            process.StartInfo.FileName = "msiexec";

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        public bool Install(string https, string tray, string server, string webRoot, string company, string rootLog, string location)
        {
            var process = new Process
            {
                StartInfo =
                {
                    Arguments =
                        $"/i \"{Path.Combine(tmpLocation, "FOGService.msi")}\" " +
                        $"/quiet " +
                        $"USETRAY=\"{tray}\" " +
                        $"WEBROOT=\"{webRoot}\" " +
                        $"ROOTLOG=\"{rootLog}\" " +
                        (string.IsNullOrEmpty(location) ? "" : $"INSTALLDIR=\"{location}\" ") +
                        $"WEBADDRESS=\"{server}\" " +
                        $"HTTPS=\"{https}\""
                }
            };
            process.StartInfo.FileName = "msiexec";

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }

        public bool Configure()
        {
            return true;
        }

        public string GetLocation()
        {
            return "";
        }

        public void PrintInfo()
        {
        }

        public bool Uninstall()
        {
            // TODO: Obtain product codes of installed version(s)
            //return ProcessHandler.Run("msiexec.exe", "/x " + ProductCode) == 0;
            throw new NotImplementedException();
        }
    }
}