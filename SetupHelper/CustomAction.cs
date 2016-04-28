/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2016 FOG Project
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
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32.TaskScheduler;
using FOG;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace SetupHelper
{
    public class CustomActions
    {
        private static string jsonFile = Path.Combine(
            Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.User),
            "session.json");
        private static void DisplayMSIError(Session session, string msg)
        {
            var r = new Record();
            r.SetString(0, msg);
            session.Message(InstallMessage.Error, r);
        }

        [CustomAction]
        public static ActionResult DumpConfig(Session session)
        {
            var properties = new string[] { "HTTPS", "USETRAY", "WEBADDRESS", "WEBROOT",
                "ROOTLOG", "INSTALLDIR", "ProductVersion" };
            try
            {
                var json = new JObject();

                foreach (var prop in properties)
                    json[prop] = session[prop];
                File.WriteAllText(jsonFile, json.ToString());
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to store configuration: " + ex.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult InstallCert(Session session)
        {
            try
            {
                var config = GetSettings();

                if (GenericSetup.PinServerCert(GetValue("INSTALLDIR", config)))
                {
                    return ActionResult.Success;
                }
                else
                {
                    DisplayMSIError(session, "Unable to install CA certificate");
                    return ActionResult.Failure;
                }
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
                var config = GetSettings();


                GenericSetup.SaveSettings(
                    GetValue("HTTPS", config),
                    GetValue("USETRAY", config),
                    GetValue("WEBADDRESS", config),
                    GetValue("WEBROOT", config),
                    "FOG",
                    GetValue("ROOTLOG", config),
                    GetValue("ProductVersion", config),
                     GetValue("INSTALLDIR", config));

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
            try
            {
                GenericSetup.UnpinServerCert();
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to remove CA certficate: " + ex.Message);
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult InstallFOGCert(Session session)
        {
            try
            {
                GenericSetup.InstallFOGCert(session.CustomActionData["CAFile"]);
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to install FOG Project CA: " + ex.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult UninstallFOGCert(Session session)
        {
            try
            {
                GenericSetup.UninstallFOGCert();
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to remove FOG Project CA certficate: " + ex.Message);
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult CheckForLegacy(Session session)
        {
            try
            {
                var software =
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall").GetSubKeyNames();

                if (software.Contains("{91C5D423-B6AB-4EAB-8F17-2BB3AE162CA1}"))
                {
                    DisplayMSIError(session, "Please uninstall the legacy client and re-run this installer");
                    return ActionResult.Failure;
                }
            }
            catch (Exception) { }

            return ActionResult.Success;
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
                taskService.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }

            return ActionResult.Success;
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
