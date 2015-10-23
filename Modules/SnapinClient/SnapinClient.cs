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
using Zazzles;
using Zazzles.Data;
using Zazzles.Middleware;
using Zazzles.Modules;

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

                if (!taskResponse.Encrypted)
                {
                    Log.Error(Name, "Response was not encrypted");
                    return;
                }

                Log.Entry(Name, "Snapin Found:");
                Log.Entry(Name, $"    ID: {taskResponse.GetField("JOBTASKID")}");
                Log.Entry(Name, $"    RunWith: {taskResponse.GetField("SNAPINRUNWITH")}");
                Log.Entry(Name, $"    RunWithArgs: {taskResponse.GetField("SNAPINRUNWITHARGS")}");
                Log.Entry(Name, $"    Name: {taskResponse.GetField("SNAPINNAME")}");
                Log.Entry(Name, $"    File: {taskResponse.GetField("SNAPINFILENAME")}");
                Log.Entry(Name, $"    Created: {taskResponse.GetField("JOBCREATION")}");
                Log.Entry(Name, $"    Args: {taskResponse.GetField("SNAPINARGS")}");
                Log.Entry(Name, $"    Reboot: {taskResponse.GetField("SNAPINBOUNCE")}");

                if (string.IsNullOrEmpty(taskResponse.GetField("SNAPINHASH")))
                {
                    Log.Error(Name, "Snapin hash does not exist");
                    return;
                }

                var snapinFilePath = Path.Combine(Settings.Location, "tmp", taskResponse.GetField("SNAPINFILENAME"));

                var downloaded =
                    Communication.DownloadFile(
                        $"/service/snapins.file.php?mac={Configuration.MACAddresses()}&taskid={taskResponse.GetField("JOBTASKID")}", snapinFilePath);

                Log.Entry(Name, snapinFilePath);
                var exitCode = "-1";

                //If the file downloaded successfully then run the snapin and report to FOG what the exit code was
                if (downloaded)
                {
                    var sha512 = Hash.SHA512(snapinFilePath);
                    if (!sha512.ToUpper().Equals(taskResponse.GetField("SNAPINHASH").ToUpper()))
                    {
                        Log.Error(Name, "Hash does not match");
                        Log.Error(Name, "--> Ideal: " + taskResponse.GetField("SNAPINHASH"));
                        Log.Error(Name, "--> Actual: " + sha512);

                        return;
                    }
                    exitCode = StartSnapin(taskResponse, snapinFilePath);
                    if (File.Exists(snapinFilePath))
                        File.Delete(snapinFilePath);

                    Communication.Contact(
                        $"/service/snapins.checkin.php?taskid={taskResponse.GetField("JOBTASKID")}&exitcode={exitCode}", true);

                    if (!taskResponse.GetField("SNAPINBOUNCE").Equals("1"))
                    {
                        if (!Power.ShuttingDown)
                        {
                            //Rerun this method to check for the next snapin
                            continue;
                        }
                    }
                    else
                    {
                        Power.Restart("Snapin requested shutdown", Power.ShutdownOptions.Delay,
                            "This computer needs to reboot to apply new software.");
                    }
                }
                else
                    Communication.Contact(
                        $"/service/snapins.checkin.php?taskid={taskResponse.GetField("JOBTASKID")}&exitcode={exitCode}", true);
                break;
            }
        }

        //Execute the snapin once it has been downloaded
        private string StartSnapin(Response taskResponse, string snapinPath)
        {
            Notification.Emit(
                "Installing " + taskResponse.GetField("SNAPINNAME"),
                "Please do not shutdown until this is completed",
                $"snapin-{taskResponse.GetField("SNAPINNAME")}",
                true);

            var process = GenerateProcess(taskResponse, snapinPath);
            try
            {
                Log.Entry(Name, "Starting snapin...");
                process.Start();
                process.WaitForExit();
                Log.Entry(Name, "Snapin finished");
                Log.Entry(Name, "Return Code: " + process.ExitCode);

                Notification.Emit(
                    taskResponse.GetField("SNAPINNAME") + " Installed",
                    "Installation has finished and is now ready for use",
                    $"snapin-{taskResponse.GetField("SNAPINNAME")}", 
                    true);

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
                    $"{taskResponse.GetField("SNAPINRUNWITHARGS").Trim()} " +
                    $"\"{snapinPath.Trim()}\" " +
                    $"{Environment.ExpandEnvironmentVariables(taskResponse.GetField("SNAPINARGS"))}"
                    .Trim());
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