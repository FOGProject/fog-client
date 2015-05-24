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
using System.ServiceProcess;
using FOG.Handlers;


namespace FOG
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var service = new ServiceController("fogservice");
            const string logName = "Update Helper";

            LogHandler.Log(logName, "Shutting down service...");
            //Stop the service
            if (service.Status == ServiceControllerStatus.Running)
                service.Stop();

            service.WaitForStatus(ServiceControllerStatus.Stopped);

            LogHandler.Log(logName, "Killing remaining FOG processes...");
            if (Process.GetProcessesByName("FOGService").Length > 0)
                foreach (var process in Process.GetProcessesByName("FOGService"))
                    process.Kill();

            LogHandler.Log(logName, "Applying MSI...");
            ApplyUpdates();

            //Start the service

            LogHandler.Log(logName, "Starting service...");
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running);
            service.Dispose();

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info");
        }

        private static void ApplyUpdates()
        {
            const string logName = "Update Helper";

            var useTray = RegistryHandler.GetSystemSetting("Tray");
            var https = RegistryHandler.GetSystemSetting("HTTPS");
            var webRoot = RegistryHandler.GetSystemSetting("WebRoot");
            var server = RegistryHandler.GetSystemSetting("Server");

            var process = new Process
            {
                StartInfo =
                {
                    Arguments = string.Format("/i \"{0}\" /quiet /USETRAY=\"{1}\" HTTPS=\"{2}\" WEBADDRESS=\"{3}\" WEBROOT=\"{4}\"", 
                        (AppDomain.CurrentDomain.BaseDirectory + "FOGService.msi"), 
                        useTray, https, server, webRoot)
                }
            };
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.StartInfo.FileName = "msiexec";


            LogHandler.Log(logName, "--> " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
            process.Start();
            process.WaitForExit();
        }
    }
}