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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FOG.Handlers.Data;
using FOG.Handlers.Middleware;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json.Linq;

namespace SetupHelper
{
    public class CustomActions
    {
        static void DisplayMSIError(Session session, string msg)
        {
            var r = new Record();
            r.SetString(0, msg);
            session.Message(InstallMessage.Error, r);
        }

        [CustomAction]
        public static ActionResult InstallCert(Session session)
        {
            var cert = RSA.GetCACertificate();
            if (cert != null) return ActionResult.Success;

            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Configuration.ServerAddress = Configuration.ServerAddress.Replace("https://", "http://");
            var keyPath = string.Format("{0}ca.cert.der", tempDirectory);
            var downloaded = Communication.DownloadFile("/management/other/ca.cert.der", keyPath);
            if (!downloaded)
            {
                DisplayMSIError(session, "Failed to download CA certificate");
                return ActionResult.Failure;
            }

            try
            {
                cert = new X509Certificate2(keyPath);
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to install CA certificate: " + ex.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult SaveSettings(Session session)
        {

            try
            {
                var settings = new JObject();
                settings.Add("HTTPS", settings["HTTPS"]);
                settings.Add("Tray", settings["USETRAY"]);
                settings.Add("Server", settings["WEBADDRESS"]);
                settings.Add("WebRoot", settings["WEBROOT"]);
                settings.Add("Version", settings["ProductVersion"]);
                settings.Add("Company", "FOG");
                File.WriteAllText(session["INSTALLLOCATION"] + @"\settings.json", settings.ToString());
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to create settings file: " + ex.Message);
            }

            return ActionResult.Failure;
        }

        [CustomAction]
        public static ActionResult UninstallCert(Session session)
        {
            var cert = RSA.GetCACertificate();
            if (cert == null) return ActionResult.Success;

            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(cert);
                store.Close();
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to remove CA certficate: " + ex.Message);
                return ActionResult.Success;
            }
            
        }

        [CustomAction]
        public static ActionResult CleanTasks(Session session)
        {
            try
            {
                var taskService = new TaskService();
                var existingTasks = taskService.GetFolder("FOG").AllTasks.ToList();

                foreach (var task in existingTasks)
                    taskService.RootFolder.DeleteTask(@"FOG\" + task.Name);

                taskService.RootFolder.DeleteFolder("FOG", false);
            }
            catch (Exception ) { }

            return ActionResult.Success;
        }
    }
}
