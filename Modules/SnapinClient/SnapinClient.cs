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


namespace FOG.Modules.SnapinClient
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

        protected override void DoWork()
        {
            while (true)
            {
                //Get task info
                var taskResponse = CommunicationHandler.GetResponse("/service/snapins.checkin.php", true);

                //Download the snapin file if there was a response and run it
                if (taskResponse.Error) return;

                LogHandler.Log(Name, "Snapin Found:");
                LogHandler.Log(Name, string.Format("    ID: {0}", taskResponse.GetField("JOBTASKID")));
                LogHandler.Log(Name, string.Format("    RunWith: {0}", taskResponse.GetField("SNAPINRUNWITH")));
                LogHandler.Log(Name, string.Format("    RunWithArgs: {0}", taskResponse.GetField("SNAPINRUNWITHARGS")));
                LogHandler.Log(Name, string.Format("    Name: {0}", taskResponse.GetField("SNAPINNAME")));
                LogHandler.Log(Name, string.Format("    File: {0}", taskResponse.GetField("SNAPINFILENAME")));
                LogHandler.Log(Name, string.Format("    Created: {0}", taskResponse.GetField("JOBCREATION")));
                LogHandler.Log(Name, string.Format("    Args: {0}", taskResponse.GetField("SNAPINARGS")));
                LogHandler.Log(Name, string.Format("    Reboot: {0}", taskResponse.GetField("SNAPINBOUNCE")));

                var snapinFilePath = string.Format("{0}tmp\\{1}", AppDomain.CurrentDomain.BaseDirectory, taskResponse.GetField("SNAPINFILENAME"));

                var downloaded = CommunicationHandler.DownloadFile(string.Format("/service/snapins.file.php?mac={0}&taskid={1}", 
                    CommunicationHandler.GetMacAddresses(), taskResponse.GetField("JOBTASKID")), snapinFilePath);
                var exitCode = "-1";

                //If the file downloaded successfully then run the snapin and report to FOG what the exit code was
                if (downloaded)
                {
                    exitCode = StartSnapin(taskResponse, snapinFilePath);
                    if (File.Exists(snapinFilePath))
                        File.Delete(snapinFilePath);

                    CommunicationHandler.Contact(string.Format("/service/snapins.checkin.php?mac={0}&taskid={1}&exitcode={2}", 
                        CommunicationHandler.GetMacAddresses(), taskResponse.GetField("JOBTASKID"), exitCode));

                    if (!taskResponse.GetField("SNAPINBOUNCE").Equals("1"))
                    {
                        if (!ShutdownHandler.ShutdownPending)
                            //Rerun this method to check for the next snapin
                            continue;
                    }
                    else
                        ShutdownHandler.Restart("Snapin requested shutdown", 30);
                }
                else
                    CommunicationHandler.Contact(string.Format("/service/snapins.checkin.php?mac={0}&taskid={1}&exitcode={2}", 
                        CommunicationHandler.GetMacAddresses(), taskResponse.GetField("JOBTASKID"), exitCode));
                break;
            }
        }

        //Execute the snapin once it has been downloaded
        private string StartSnapin(Response taskResponse, string snapinPath)
        {
            NotificationHandler.Notifications.Add(new Notification(taskResponse.GetField("SNAPINNAME"),
                string.Format("FOG is installing {0}", taskResponse.GetField("SNAPINNAME")), 10));

            var process = GenerateProcess(taskResponse, snapinPath);

            try
            {
                LogHandler.Log(Name, "Starting snapin...");
                process.Start();
                process.WaitForExit();
                LogHandler.Log(Name, "Snapin finished");
                LogHandler.Log(Name, "Return Code: " + process.ExitCode);
                
                NotificationHandler.Notifications.Add(new Notification(
                    string.Format("Finished {0}", taskResponse.GetField("SNAPINNAME")),
                    taskResponse.GetField("SNAPINNAME") + " finished installing", 10));

                return process.ExitCode.ToString();
            }
            catch (Exception ex)
            {
                LogHandler.Error(Name, "Could not start snapin");
                LogHandler.Error(Name, ex);
            }

            return "-1";
        }

        //Create a proccess to run the snapin with
        private static Process GenerateProcess(Response taskResponse, string snapinPath)
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
                    string.Format("{0} \"{1} \"{2}", taskResponse.GetField("SNAPINRUNWITHARGS"), 
                        snapinPath, Environment.ExpandEnvironmentVariables(taskResponse.GetField("SNAPINARGS"))));
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