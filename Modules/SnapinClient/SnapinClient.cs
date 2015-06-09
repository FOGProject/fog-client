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
using FOG.Handlers.Middleware;
using FOG.Handlers.Power;


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
        }

        protected override void DoWork()
        {
            while (true)
            {
                //Get task info
                var taskResponse = Communication.GetResponse("/service/snapins.checkin.php", true);

                //Download the snapin file if there was a response and run it
                if (taskResponse.Error) return;

                Log.Entry(Name, "Snapin Found:");
                Log.Entry(Name, string.Format("    ID: {0}", taskResponse.GetField("JOBTASKID")));
                Log.Entry(Name, string.Format("    RunWith: {0}", taskResponse.GetField("SNAPINRUNWITH")));
                Log.Entry(Name, string.Format("    RunWithArgs: {0}", taskResponse.GetField("SNAPINRUNWITHARGS")));
                Log.Entry(Name, string.Format("    Name: {0}", taskResponse.GetField("SNAPINNAME")));
                Log.Entry(Name, string.Format("    File: {0}", taskResponse.GetField("SNAPINFILENAME")));
                Log.Entry(Name, string.Format("    Created: {0}", taskResponse.GetField("JOBCREATION")));
                Log.Entry(Name, string.Format("    Args: {0}", taskResponse.GetField("SNAPINARGS")));
                Log.Entry(Name, string.Format("    Reboot: {0}", taskResponse.GetField("SNAPINBOUNCE")));

                var snapinFilePath = string.Format("{0}tmp\\{1}", AppDomain.CurrentDomain.BaseDirectory, taskResponse.GetField("SNAPINFILENAME"));

                var downloaded = Communication.DownloadFile(string.Format("/service/snapins.file.php?mac={0}&taskid={1}", 
                    Configuration.MACAddresses(), taskResponse.GetField("JOBTASKID")), snapinFilePath);

                var exitCode = "-1";

                //If the file downloaded successfully then run the snapin and report to FOG what the exit code was
                if (downloaded)
                {
                    exitCode = StartSnapin(taskResponse, snapinFilePath);
                    if (File.Exists(snapinFilePath))
                        File.Delete(snapinFilePath);

                    Communication.Contact(string.Format("/service/snapins.checkin.php?taskid={0}&exitcode={1}", 
                        taskResponse.GetField("JOBTASKID"), exitCode), true);

                    if (!taskResponse.GetField("SNAPINBOUNCE").Equals("1"))
                        if (!Power.ShuttingDown)
                            //Rerun this method to check for the next snapin
                            continue;
                    else
                        Power.Restart("Snapin requested shutdown", 30);
                }
                else
                    Communication.Contact(string.Format("/service/snapins.checkin.php?taskid={0}&exitcode={1}",
                       taskResponse.GetField("JOBTASKID"), exitCode), true);
                break;
            }
        }

        //Execute the snapin once it has been downloaded
        private string StartSnapin(Response taskResponse, string snapinPath)
        {
            var notification = new Notification(taskResponse.GetField("SNAPINNAME"),
                string.Format("FOG is installing {0}", taskResponse.GetField("SNAPINNAME")), 10);

            Bus.Emit(Bus.Channel.Notification, notification.GetJson(), true);

            var process = GenerateProcess(taskResponse, snapinPath);

            try
            {
                Log.Entry(Name, "Starting snapin...");
                process.Start();
                process.WaitForExit();
                Log.Entry(Name, "Snapin finished");
                Log.Entry(Name, "Return Code: " + process.ExitCode);

                notification = new Notification(
                    string.Format("Finished {0}", taskResponse.GetField("SNAPINNAME")),
                    taskResponse.GetField("SNAPINNAME") + " finished installing", 10);

                Bus.Emit(Bus.Channel.Notification, notification.GetJson(), true);
                return process.ExitCode.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not start snapin");
                Log.Error(Name, ex);
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