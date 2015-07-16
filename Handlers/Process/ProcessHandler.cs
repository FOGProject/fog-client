using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

//TODO: Look into possible memory leaks caused by un-dispossed processes
namespace FOG.Handlers
{
    public static class ProcessHandler
    {
        private const string LogName = "Process";


        public static int RunClientEXE(string filePath, string param, bool wait = true)
        {
            return RunEXE(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),filePath), param, wait);    
        }

        public static int RunEXE(string filePath, string param, bool wait = true)
        {
            filePath = (Settings.OS == Settings.OSType.Windows)
                ? filePath
                : "mono";

            param = (Settings.OS == Settings.OSType.Windows)
                ? param
                : ("\"" + filePath + "\" " + param);

            return Run(filePath, param, wait);
        }

        public static int Run(string filePath, string param, bool wait = false)
        {
            Log.Debug(LogName, "Running process...");
            Log.Debug(LogName, "--> Filepath:   " + filePath);
            Log.Debug(LogName, "--> Parameters: " + param);
            Log.Debug(LogName, "--> Wait:       " + wait);
            var returnCode = -1;

            try
            {
                using (var process = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        FileName = (Settings.OS == Settings.OSType.Windows)
                            ? filePath
                            : "mono",
                        Arguments = (Settings.OS == Settings.OSType.Windows)
                            ? param
                            : ("\"" + filePath + "\" " + param)
                    }

                })
                {
                    process.Start();
                    if (!wait) return returnCode;

                    process.WaitForExit();
                    returnCode = process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to run process");
                Log.Error(LogName, ex);
            }

            Log.Entry(LogName, "Return code = " + returnCode);
            return returnCode;
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

            try
            {
                var processes = GetMonoProcesses(name);
                foreach (var process in processes)
                    process.Kill();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to kill all exes named " + name);
                Log.Error(LogName, ex);
            }
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

            try
            {
                var processes = GetMonoProcesses(name);
                if (processes.Count > 0)
                    processes[0].Kill();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to kill exe named " + name);
                Log.Error(LogName, ex);
            }
        }

        private static List<Process> GetMonoProcesses(string name)
        {
            var processes = new List<Process>();

            try
            {
                var monoProcesses = Process.GetProcessesByName("mono");
                foreach (var process in monoProcesses)
                {
                    //TODO: Get PID of each process and use /proc/PID/cmdline to retrieve EXE name
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to get mono processes called " + name);
                Log.Error(LogName, ex);
            }

            return processes;
        }
    }
}
