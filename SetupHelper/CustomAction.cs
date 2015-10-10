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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32.TaskScheduler;
using FOG;

namespace SetupHelper
{
    public class CustomActions
    {
        private static void DisplayMSIError(Session session, string msg)
        {
            var r = new Record();
            r.SetString(0, msg);
            session.Message(InstallMessage.Error, r);
        }

        [CustomAction]
        public static ActionResult InstallCert(Session session)
        {
            try
            {
                if (session["LIGHT"].Equals("1"))
                    return ActionResult.Success;

                return GenericSetup.PinServerCert(session["INSTALLLOCATION"]) ? ActionResult.Success : ActionResult.Failure;
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
                if (session["LIGHT"].Equals("1"))
                    return ActionResult.Success;

                GenericSetup.SaveSettings(
                    session["HTTPS"],
                    session["USETRAY"],
                    session["WEBADDRESS"],
                    session["WEBROOT"],
                    session["ProductVersion"],
                    "FOG",
                    session["INSTALLLOCATION"],
                    session["ROOTLOG"]);

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
                if (session["LIGHT"].Equals("1"))
                    return ActionResult.Success;

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
                if (session["LIGHT"].Equals("1"))
                    return ActionResult.Success;

                var cert = new X509Certificate2(session.CustomActionData["CAFile"]);
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();

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
            var cert = new X509Certificate2();
            try
            {
                if (session["LIGHT"].Equals("1"))
                    return ActionResult.Success;

                X509Certificate2 CAroot = null;
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var cers = store.Certificates.Find(X509FindType.FindBySubjectName, "FOG Project", true);

                if (cers.Count > 0)
                {
                    CAroot = cers[0];
                }
                store.Close();

                cert = CAroot;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to remove FOG Project CA certficate: " + ex.Message);
                return ActionResult.Success;
            }

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
                if (session["LIGHT"].Equals("1"))
                    return ActionResult.Success;

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
