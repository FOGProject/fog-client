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
using System.IO;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32.TaskScheduler;
using FOG;
using Microsoft.Win32;
using Zazzles;

namespace SetupHelper
{
    public class CustomActions
    {
        private const string LogName = "Installer";

        private static void DisplayMSIError(Session session, string msg)
        {
            var r = new Record();
            r.SetString(0, msg);
            session.Message(InstallMessage.Error, r);
            Log.Error(LogName, msg);
        }

        [CustomAction]
        public static ActionResult InstallCert(Session session)
        {
            try
            {
                if (GenericSetup.PinServerCert((session.CustomActionData["sHTTPS"].Equals("1")) ? "1" : "0",
                    session.CustomActionData["sWEBADDRESS"], 
                    session.CustomActionData["sWEBROOT"], 
                    session.CustomActionData["sINSTALLDIR"]))
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
                var sHttps = session.CustomActionData["sHTTPS"];
                GenericSetup.SaveSettings((sHttps.Equals("1")) ? "1" : "0", session.CustomActionData["sUSETRAY"],
                    session.CustomActionData["sWEBADDRESS"], session.CustomActionData["sWEBROOT"], "FOG",
                    session.CustomActionData["sROOTLOG"], session.CustomActionData["sProductVersion"], session.CustomActionData["sINSTALLDIR"]);

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to create settings file: " + ex.Message);
            }

            return ActionResult.Failure;
        }

        [CustomAction]
        public static ActionResult Cleanup(Session session)
        {
            try
            {
                var dir = session.CustomActionData["sINSTALLDIR"];

                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to cleanup leftover files: " + ex.Message);
            }

            return ActionResult.Success;
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
    }
}
