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
using FOG.Handlers;

namespace FOG.Modules
{
    /// <summary>
    ///     Installs snapins on client computers
    /// </summary>
    public class SnapinClient : AbstractModule
    {
        public SnapinClient()
        {
            Name = "SnapinClient";
            Description = "Installs snapins on client computers";
        }

        protected override void doWork()
        {
            while (true)
            {
                //Get task info
                var taskResponse = CommunicationHandler.GetResponse("/service/snapins.checkin.php", true);


                //Download the snapin file if there was a response and run it
                if (taskResponse.Error) return;

                LogHandler.Log(Name, "Snapin Found:");
                LogHandler.Log(Name, "    ID: " + taskResponse.GetField("JOBTASKID"));
                LogHandler.Log(Name, "    RunWith: " + taskResponse.GetField("SNAPINRUNWITH"));
                LogHandler.Log(Name, "    RunWithArgs: " + taskResponse.GetField("SNAPINRUNWITHARGS"));
                LogHandler.Log(Name, "    Name: " + taskResponse.GetField("SNAPINNAME"));
                LogHandler.Log(Name, "    File: " + taskResponse.GetField("SNAPINFILENAME"));
                LogHandler.Log(Name, "    Created: " + taskResponse.GetField("JOBCREATION"));
                LogHandler.Log(Name, "    Args: " + taskResponse.GetField("SNAPINARGS"));
                LogHandler.Log(Name, "    Reboot: " + taskResponse.GetField("SNAPINBOUNCE"));

                var snapinFilePath = AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + taskResponse.GetField("SNAPINFILENAME");

                var downloaded = CommunicationHandler.DownloadFile("/service/snapins.file.php?mac=" + CommunicationHandler.GetMacAddresses() + "&taskid=" + taskResponse.GetField("JOBTASKID"), snapinFilePath);
                var exitCode = "-1";

                //If the file downloaded successfully then run the snapin and report to FOG what the exit code was
                if (downloaded)
                {
                    exitCode = startSnapin(taskResponse, snapinFilePath);
                    if (File.Exists(snapinFilePath))
                        File.Delete(snapinFilePath);

                    CommunicationHandler.Contact("/service/snapins.checkin.php?mac=" + CommunicationHandler.GetMacAddresses() + "&taskid=" + taskResponse.GetField("JOBTASKID") + "&exitcode=" + exitCode);

                    if (taskResponse.GetField("SNAPINBOUNCE").Equals("1"))
                    {
                        ShutdownHandler.Restart("Snapin requested shutdown", 30);
                    }
                    else if (!ShutdownHandler.ShutdownPending)
                    {
                        //Rerun this method to check for the next snapin
                        continue;
                    }
                }
                else
                {
                    CommunicationHandler.Contact("/service/snapins.checkin.php?mac=" + CommunicationHandler.GetMacAddresses() + "&taskid=" + taskResponse.GetField("JOBTASKID") + "&exitcode=" + exitCode);
                }
                break;
            }
        }

        //Execute the snapin once it has been downloaded
        private string startSnapin(Response taskResponse, string snapinPath)
        {
            NotificationHandler.Notifications.Add(new Notification(taskResponse.GetField("SNAPINNAME"),
                "FOG is installing " +
                taskResponse.GetField("SNAPINNAME"), 10));

            var process = generateProcess(taskResponse, snapinPath);

            try
            {
                LogHandler.Log(Name, "Starting snapin...");
                process.Start();
                process.WaitForExit();
                LogHandler.Log(Name, "Snapin finished");
                LogHandler.Log(Name, "Return Code: " + process.ExitCode);
                NotificationHandler.Notifications.Add(new Notification(
                    "Finished " + taskResponse.GetField("SNAPINNAME"),
                    taskResponse.GetField("SNAPINNAME") + " finished installing", 10));
                return process.ExitCode.ToString();
            }
            catch (Exception ex)
            {
                LogHandler.Log(Name, "Error starting snapin");
                LogHandler.Log(Name, "ERROR: " + ex.Message);
            }

            return "-1";
        }

        //Create a proccess to run the snapin with
        private Process generateProcess(Response taskResponse, string snapinPath)
        {
            var process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            //Check if the snapin run with field was specified
            if (!taskResponse.GetField("SNAPINRUNWITH").Equals(""))
            {
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(
                    taskResponse.GetField("SNAPINRUNWITH"));

                process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(
                    taskResponse.GetField("SNAPINRUNWITHARGS"));

                process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(
                    taskResponse.GetField("SNAPINRUNWITHARGS") + " \"" + snapinPath + " \"" +
                    Environment.ExpandEnvironmentVariables(taskResponse.GetField("SNAPINARGS")));
            }
            else
            {
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(snapinPath);

                process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(
                    taskResponse.GetField("SNAPINARGS"));
            }

            return process;
        }
    }
}