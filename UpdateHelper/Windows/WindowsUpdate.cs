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

using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using Zazzles;
using Zazzles.Modules.Updater;

namespace FOG
{
    internal class WindowsUpdate : IUpdate
    {
        private const string LogName = "UpdateHelper";

        public void ApplyUpdate()
        {
            var useTray = Settings.Get("Tray");
            var https = Settings.Get("HTTPS");
            var webRoot = Settings.Get("WebRoot");
            var server = Settings.Get("Server");
            var logRoot = Settings.Get("RootLog");

            var process = new Process
            {
                StartInfo =
                {
                    Arguments =
                        $"/i \"{Path.Combine(Settings.Location, "FOGService.msi")}\" " +
                        $"/quiet " +
                        $"USETRAY=\"{useTray}\" " +
                        $"HTTPS=\"{https}\" " +
                        $"WEBADDRESS=\"{server}\" " +
                        $"WEBROOT=\"{webRoot}\" " +
                        $"ROOTLOG=\"{logRoot}\""
                }
            };
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.StartInfo.FileName = "msiexec";

            Log.Entry(LogName, "--> " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
            process.Start();
            process.WaitForExit();
        }

        public void StartService()
        {
            using (var service = new ServiceController("fogservice"))
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
        }

        public void StopService()
        {
            using (var service = new ServiceController("fogservice"))
            {
                if (service.Status == ServiceControllerStatus.Running)
                    service.Stop();

                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }
    }
}