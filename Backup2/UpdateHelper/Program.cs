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
using System.ServiceProcess;
using System.Diagnostics;
using FOG.Handlers;

namespace FOG
{
    class Program
    {
		
        public static void Main(string[] args)
        {
            var service = new ServiceController("fogservice");
            String LOG_NAME = "Update Helper";
			
            LogHandler.Log(LOG_NAME, "Shutting down service...");
            //Stop the service
            if (service.Status == ServiceControllerStatus.Running)
                service.Stop();
			
            service.WaitForStatus(ServiceControllerStatus.Stopped);
			
            LogHandler.Log(LOG_NAME, "Killing remaining FOG processes...");
            if (Process.GetProcessesByName("FOGService").Length > 0)
            {
                foreach (Process process in Process.GetProcessesByName("FOGService"))
                {
                    process.Kill();
                }
            }
			
            LogHandler.Log(LOG_NAME, "Applying MSI...");
            applyUpdates();
			
            //Start the service

            LogHandler.Log(LOG_NAME, "Starting service...");
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running);
            service.Dispose();
			
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info");
			
        }
		
        private static void applyUpdates()
        {
            String LOG_NAME = "Update Helper";
            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			
            process.StartInfo.FileName = "msiexec";
            ;
            process.StartInfo.Arguments = "/i \"" + (AppDomain.CurrentDomain.BaseDirectory + "FOGService.msi") + "\" /quiet";
            LogHandler.Log(LOG_NAME, "--> " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
            process.Start();
            process.WaitForExit();
        }
    }
}