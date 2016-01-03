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
using Zazzles;
using Zazzles.Data;
using Zazzles.Modules;

namespace FOG.Modules.SnapinClient
{
    /// <summary>
    ///     Installs snapins on client computers
    /// </summary>
    public sealed class SnapinClient : AbstractModule<SnapinMessage>
    {
        public override string Name { get; protected set; }
        public override Settings.OSType Compatiblity { get; protected set; }
        public override EventProcessorType Type { get; protected set; }

        public SnapinClient()
        {
            Name = "SnapinClient";
            Compatiblity = Settings.OSType.All;
            Type = EventProcessorType.Synchronous;
        }

        //Execute the snapin once it has been downloaded
        private string StartSnapin(SnapinMessage data, string snapinPath)
        {
            //Notification.Emit(
            //    "Installing " + data["SNAPINNAME"],
            //    "Please do not shutdown until this is completed",
            //    $"snapin-{ data["SNAPINNAME"]}",
            //    true);

            var process = GenerateProcess(data, snapinPath);
            try
            {
                Log.Entry(Name, "Starting snapin...");
                process.Start();
                process.WaitForExit();
                Log.Entry(Name, "Snapin finished");
                Log.Entry(Name, "Return Code: " + process.ExitCode);

                //Notification.Emit(
                //     data["SNAPINNAME"] + " Installed",
                //    "Installation has finished and is now ready for use",
                //    $"snapin-{ data["SNAPINNAME"]}", 
                //    true);

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
        private static Process GenerateProcess(SnapinMessage data, string snapinPath)
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
            if (!data.RunWith.Equals(""))
            {
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(data.RunWith);
                process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(
                    $"{ data.RunWithArgs} " +
                    $"\"{snapinPath.Trim()}\" " +
                    $"{Environment.ExpandEnvironmentVariables(data.Args)}"
                    .Trim());
            }
            else
            {
                process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(snapinPath);
                process.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(data.Args);
            }

            return process;
        }

        protected override void OnEvent(SnapinMessage message)
        {
            Log.Entry(Name, "Snapin Found:");
            Log.Entry(Name, "    Name: " + message.Name);
            Log.Entry(Name, "    RunWith: " + message.RunWith);
            Log.Entry(Name, "    RunWithArgs: " + message.RunWithArgs);
            Log.Entry(Name, "    File: " + message.File);
            Log.Entry(Name, "    Args: " + message.Args);
            Log.Entry(Name, "    Restart: " + message.Restart);

            if (string.IsNullOrEmpty(message.Hash))
            {
                Log.Error(Name, "Snapin hash does not exist");
                return;
            }

            var snapinFilePath = Path.Combine(Settings.Location, "tmp", message.File);

            Log.Entry(Name, snapinFilePath);

            //TODO: ADD NEW DOWNLAOD PATH
            var downloaded = true;
            if (!downloaded) return;

            var sha512 = Hash.SHA512(snapinFilePath);
            if (!sha512.ToUpper().Equals(message.Hash.ToUpper()))
            {
                Log.Error(Name, "Hash does not match");
                Log.Error(Name, "--> Ideal: " + message.Hash);
                Log.Error(Name, "--> Actual: " + sha512);

                return;
            }

            var exitCode = StartSnapin(message, snapinFilePath);
            if (File.Exists(snapinFilePath))
                File.Delete(snapinFilePath);

            if (message.Restart)
            {
                Power.Restart("Snapin requested shutdown", Power.ShutdownOptions.Delay,
                    "This computer needs to reboot to apply new software.");
            }
        }

    }
}