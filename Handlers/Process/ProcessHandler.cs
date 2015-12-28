/*
 * FOG Service : A cross platform service framework
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

namespace FOG.Handlers
{
    public static class ProcessHandler
    {
        private const string LogName = "Process";

        /// <summary>
        ///     Get the output of a command
        /// </summary>
        /// <param name="filePath">The file to execute</param>
        /// <param name="args">The arguments to use</param>
        /// <returns>An array of the lines outputed</returns>
        public static string[] GetOutput(string filePath, params string[] args)
        {
            var procInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = string.Join(" ", args),
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            Log.Debug(LogName, "Running process...");
            Log.Debug(LogName, "--> Filepath:   " + procInfo.FileName);
            Log.Debug(LogName, "--> Parameters: " + procInfo.Arguments);

            using (var proc = new Process { StartInfo =  procInfo })
            {
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                return output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            }
        }

        /// <summary>
        ///     Run a process
        /// </summary>
        /// <param name="filePath">The path of the executable to run</param>
        /// <param name="wait">Wait for the process to exit</param>
        /// <param name="args">Parameters to run the process with</param>
        /// <returns>The exit code of the process. Will be -1 if wait is false.</returns>
        public static int Run(string filePath, bool wait, params string[] args)
        {
            var procInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = string.Join(" ", args),
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Log.Debug(LogName, "Running process...");
            Log.Debug(LogName, "--> Filepath:   " + procInfo.FileName);
            Log.Debug(LogName, "--> Parameters: " + procInfo.Arguments);

            try
            {
                using (var proc = new Process { StartInfo = procInfo })
                {
                    proc.Start();

                    if (wait)
                        proc.WaitForExit();
                    if (!proc.HasExited)
                        return -1;

                    Log.Entry(LogName, $"--> Exit Code = {proc.ExitCode}");
                    return proc.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to run process");
                Log.Error(LogName, ex);
            }

            return -1;
        }
    }
}