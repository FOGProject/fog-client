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
using FOG;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32.TaskScheduler;

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
                return MonoHelper.PinCert(session["INSTALLLOCATION"]) ? ActionResult.Success : ActionResult.Failure;
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
                MonoHelper.SaveSettings(
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
                MonoHelper.UnpinCert();
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to remove CA certficate: " + ex.Message);
            }

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