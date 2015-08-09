﻿/*
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

namespace FOG.Handlers
{
    public static class ProcessHandler
    {
        private const string LogName = "Process";

        public static int RunClientEXE(string filePath, string param, bool wait = true)
        {
            return RunEXE(Path.Combine(Settings.Location, filePath), param, wait);    
        }

        public static int RunEXE(string filePath, string param, bool wait = true)
        {
            if (Settings.OS != Settings.OSType.Windows)
            {
                param = "mono " + filePath + " " + param;
                param = param.Trim();

                var proc = new Process();
                var info = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    RedirectStandardInput = true,
                    UseShellExecute = false
                };

                proc.StartInfo = info;
                proc.Start();

                using (var sw = proc.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine("export DISPLAY=:0;"+ param);
                    }
                }


                if (!proc.HasExited) return -1;
                return proc.ExitCode;

            }


            return Run(filePath, param, wait);
        }

        public static int Run(string filePath, string param,  bool wait = true)
        {
            Log.Debug(LogName, "Running process...");
            Log.Debug(LogName, "--> Filepath:   " + filePath);
            Log.Debug(LogName, "--> Parameters: " + param);

            try
            {
                using (var process = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        FileName = filePath,
                        Arguments = param
                    }

                })
                {
                    process.Start();
                    if (wait)
                        process.WaitForExit();

                    if (!process.HasExited) return -1;

                    Log.Debug(LogName, $"--> Exit Code = {process.ExitCode}");
                    return process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to run process");
                Log.Error(LogName, ex);
            }

            return -1;
        }

        public static Process CreateImpersonatedClientEXE(string filePath, string param, string user)
        {
            var fileName = "su";
            var arguments = "- " + user + " -c \"mono " + Path.Combine(Settings.Location, filePath) + " " + param;
            arguments = arguments.Trim();
            arguments = arguments + "\"";


            Log.Debug(LogName, "Creating impersonated process...");
            Log.Debug(LogName, "--> Filepath:   " + fileName);
            Log.Debug(LogName, "--> Parameters: " + arguments);

            var proc = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = arguments
                }
            };

            return proc;
        }

        public static void KillAll(string name)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(name))
                    process.Kill();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not kill all processes named " + name);
                Log.Error(LogName, ex);
            }
        }

        public static void KillAllEXE(string name)
        {
            if (Settings.OS == Settings.OSType.Windows)
            {
                KillAll(name);
                return;
            }

            Run("pkill", "-f " + name);
        }

        public static void Kill(string name)
        {
            try
            {
                var processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                {
                    processes[0].Kill();
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to kill process " + name);
                Log.Error(LogName, ex);
            }
        }

        public static void KillEXE(string name)
        {
            if (Settings.OS == Settings.OSType.Windows)
            {
                Kill(name);
                return;
            }

            Run("pgrep " + name + " | while read -r line; do kill $line; exit; done", "");
        }
    }
}