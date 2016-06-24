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
using System.Diagnostics;
using System.IO;
using FOG.Modules.DataContracts;
using Zazzles;
using Zazzles.Data;
using Zazzles.Middleware;
using Zazzles.Modules;

namespace FOG.Modules.SnapinClient
{
    /// <summary>
    ///     Installs snapins on client computers
    /// </summary>
    public class SnapinClient : AbstractModule<DataContracts.SnapinClient>
    {
        public SnapinClient()
        {
            Name = "SnapinClient";
        }

        protected override void DoWork(Response data, DataContracts.SnapinClient msg)
        {
            if (data.Error) return;

            if (!data.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            foreach (var snapin in msg.Snapins)
            {
                Log.Entry(Name, "Snapin Found:");
                Log.Entry(Name, $"    ID: {snapin.JobTaskID}");
                Log.Entry(Name, $"    Name: {snapin.Name}");
                Log.Entry(Name, $"    Created: {snapin.JobCreation}");
                Log.Entry(Name, $"    Action: {snapin.Action}");
                Log.Entry(Name, $"    Hide: {snapin.Hide}");
                Log.Entry(Name, $"    TimeOut: {snapin.TimeOut}");
                
                if (!snapin.Hide)
                {
                    Log.Entry(Name, $"    RunWith: {snapin.RunWith}");
                    Log.Entry(Name, $"    RunWithArgs: {snapin.RunWithArgs}");
                    Log.Entry(Name, $"    File: {snapin.FileName}");
                    Log.Entry(Name, $"    Args: {snapin.Args}");
                }


                if (string.IsNullOrEmpty(snapin.Hash))
                {
                    Log.Error(Name, "Snapin hash does not exist");
                    return;
                }

                var snapinFilePath = Path.Combine(Settings.Location, "tmp", snapin.FileName);

                var downloaded =
                    Communication.DownloadFile(
                        $"/service/snapins.file.php?mac={Configuration.MACAddresses()}&taskid={snapin.JobTaskID}", snapinFilePath);

                Log.Entry(Name, snapinFilePath);
                var exitCode = "-1";

                //If the file downloaded successfully then run the snapin and report to FOG what the exit code was
                if (!downloaded)
                {
                    Communication.Contact(
                        $"/service/snapins.checkin.php?taskid={snapin.JobTaskID}&exitcode={exitCode}", true);
                    return;
                }

                var sha512 = Hash.SHA512(snapinFilePath);
                if (!sha512.ToUpper().Equals(snapin.Hash.ToUpper()))
                {
                    Log.Error(Name, "Hash does not match");
                    Log.Error(Name, "--> Ideal: " + snapin.Hash);
                    Log.Error(Name, "--> Actual: " + sha512);
                    Communication.Contact($"/service/snapins.checkin.php?taskid={snapin.JobTaskID}&exitcode={exitCode}", true);
                    return;
                }

                exitCode = StartSnapin(snapin, snapinFilePath);
                if (File.Exists(snapinFilePath))
                    File.Delete(snapinFilePath);

                Communication.Contact(
                    $"/service/snapins.checkin.php?taskid={snapin.JobTaskID}&exitcode={exitCode}", true);

                if (snapin.Action.Equals("reboot", StringComparison.OrdinalIgnoreCase))
                {
                    Power.Restart("Snapin requested restart", Power.ShutdownOptions.Delay,
                        "This computer needs to reboot to apply new software.");
                    break;
                }
                else if (snapin.Action.Equals("shutdown", StringComparison.OrdinalIgnoreCase))
                {
                    Power.Shutdown("Snapin requested shutdown", Power.ShutdownOptions.Delay,
                        "This computer needs to shutdown to apply new software.");
                    break;
                }

            }
        }

        //Execute the snapin once it has been downloaded
        private string StartSnapin(Snapin snapin, string snapinPath)
        {
            Notification.Emit(
                "Installing " + snapin.Name,
                "Please do not shutdown until this is completed",
                true);

            using (var process = GenerateProcess(snapin, snapinPath))
            {
                try
                {
                    Log.Entry(Name, "Starting snapin...");
                    process.Start();

                    if (snapin.TimeOut > 0)
                    {
                        process.WaitForExit(snapin.TimeOut*1000);
                        if (!process.HasExited)
                        {
                            Log.Entry(Name, "Snapin has exceeded the timeout, killing the process");
                            process.Kill();
                        }
                    }
                    else
                    {
                        process.WaitForExit();
                    }
                    Log.Entry(Name, "Snapin finished");
                    var returnCode = process.ExitCode;

                    Log.Entry(Name, "Return Code: " + returnCode);

                    Notification.Emit(
                        snapin.Name + " Installed",
                        "Installation has finished and is now ready for use",
                        true);

                    return returnCode.ToString();
                }
                catch (Exception ex)
                {
                    Log.Error(Name, "Could not run snapin");
                    Log.Error(Name, ex);
                }
            }

            return "-1";
        }

        //Create a proccess to run the snapin with
        private static Process GenerateProcess(Snapin snapin, string snapinPath)
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
            if (!snapin.RunWith.Equals(""))
            {
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(
                    snapin.RunWith);

                process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(
                    $"{snapin.RunWithArgs.Trim()} " +
                    $"\"{snapinPath.Trim()}\" " +
                    $"{Environment.ExpandEnvironmentVariables(snapin.Args)}"
                    .Trim());
            }
            else
            {
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(snapinPath);
                process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(snapin.Args);
            }

            return process;
        }
    }
}