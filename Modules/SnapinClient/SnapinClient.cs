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
using System.Diagnostics;
using System.IO;
using FOG.Modules.DataContracts;
using Zazzles;
using Zazzles.Data;
using Zazzles.Middleware;
using Zazzles.Modules;
using ICSharpCode.SharpZipLib.Zip;

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
            ShutdownFriendly = false;
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
                Log.Entry(Name, "Running snapin " + snapin.Name);
                Log.Debug(Name, "Snapin Found:");
                Log.Debug(Name, $"    ID: {snapin.JobTaskID}");
                Log.Debug(Name, $"    Name: {snapin.Name}");
                Log.Debug(Name, $"    Created: {snapin.JobCreation}");
                Log.Debug(Name, $"    Action: {snapin.Action}");
                Log.Debug(Name, $"    Pack: {snapin.Pack}");
                Log.Debug(Name, $"    Hide: {snapin.Hide}");
                Log.Debug(Name, $"    Server: {snapin.Url}");
                Log.Debug(Name, $"    TimeOut: {snapin.TimeOut}");

                if (snapin.Pack) {
                    Log.Debug(Name, $"    SnapinPack File: {snapin.RunWith}");
                    Log.Debug(Name, $"    SnapinPack Args: {snapin.RunWithArgs}");
                } else {
                    Log.Debug(Name, $"    RunWith: {snapin.RunWith}");
                    Log.Debug(Name, $"    RunWithArgs: {snapin.RunWithArgs}");
                    Log.Debug(Name, $"    Args: {snapin.Args}");
                }
                Log.Debug(Name, $"    File: {snapin.FileName}");

                if (string.IsNullOrEmpty(snapin.Hash))
                {
                    Log.Error(Name, "Snapin hash does not exist");
                    return;
                }

                var snapinFilePath = Path.Combine(Settings.Location, "tmp", snapin.FileName);

                var postfix = $"/service/snapins.file.php?mac={Configuration.MACAddresses()}&taskid={snapin.JobTaskID}";

                var downloaded = (string.IsNullOrWhiteSpace(snapin.Url))
                    ? Communication.DownloadFile(postfix, snapinFilePath)
                    : Communication.DownloadExternalFile(snapin.Url + postfix, snapinFilePath);


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

                exitCode = (snapin.Pack) ? ProcessSnapinPack(snapin, snapinFilePath) : StartSnapin(snapin, snapinFilePath);
                
                try
                {
                    if (File.Exists(snapinFilePath))
                        File.Delete(snapinFilePath);
                } catch (Exception ex)
                {
                    Log.Error(Name, "Unable to clean up snapin file");
                    Log.Error(Name, ex);
                }


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

        private string ProcessSnapinPack(Snapin snapin, string localPath)
        {
            var returnCode = "-1";
            Log.Entry(Name, "Processing SnapinPack " + snapin.FileName);
            var extractionPath = Path.Combine(Settings.Location, "tmp", snapin.Name);
            try
            {
                Log.Entry(Name, "Extracting SnapinPack");
                if (Directory.Exists(extractionPath))
                    Directory.Delete(extractionPath, true);
                Directory.CreateDirectory(extractionPath);
                var fz = new FastZip();
                ZipStrings.CodePage = 850;
                fz.CreateEmptyDirectories = true;
                fz.ExtractZip(localPath, extractionPath, null);

                Log.Debug(Name, "Processing SnapinPack settings");
                snapin.RunWith = snapin.RunWith.Replace("[FOG_SNAPIN_PATH]", extractionPath);
                snapin.RunWithArgs = snapin.RunWithArgs.Replace("[FOG_SNAPIN_PATH]", extractionPath);
                snapin.Args = "";

                Log.Debug(Name, "New SnapinPack File: " + snapin.RunWith);
                Log.Debug(Name, "New SnapinPack Args: " + snapin.RunWithArgs);

                returnCode = StartSnapin(snapin, extractionPath, true);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(extractionPath))
                        Directory.Delete(extractionPath, true);
                }
                catch (Exception ex)
                {
                    Log.Error(Name, "Unable to clean up snapin pack");
                    Log.Error(Name, ex);
                }
            }

            return returnCode;
        }

        //Execute the snapin once it has been downloaded
        private string StartSnapin(Snapin snapin, string snapinPath, bool snapinPack = false)
        {
            Notification.Emit(
                string.Format(SnapinStrings.INSTALLING_NOTIFICATION_TITLE, snapin.Name),
                SnapinStrings.INSTALLING_NOTIFICATION_BODY,
                true);

            using (var process = (snapinPack) 
                ? GenerateSnapinPackProcess(snapin, snapinPath) 
                : GenerateProcess(snapin, snapinPath))
            {
                try
                {
                    Log.Entry(Name, "Starting snapin");
                    process.StartInfo.EnvironmentVariables.Add("FOG_URL", Configuration.ServerAddress);
                    process.StartInfo.EnvironmentVariables.Add("FOG_SNAPIN_TASK_ID", snapin.JobTaskID.ToString());
                    process.StartInfo.EnvironmentVariables.Add("FOG_MAC_ADDRESSES", Configuration.MACAddresses());
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
                        string.Format(SnapinStrings.COMPLETE_NOTIFICATION_TITLE, snapin.Name),
                        SnapinStrings.COMPLETE_NOTIFICATION_BODY,
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
                    WorkingDirectory = Path.GetDirectoryName(snapinPath) ?? "",
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

        private static Process GenerateSnapinPackProcess(Snapin snapin, string directory)
        {
            var process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = directory,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(snapin.RunWith);
            process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(snapin.RunWithArgs).Trim();

            return process;
        }
    }
}