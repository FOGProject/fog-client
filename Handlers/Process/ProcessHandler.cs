using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FOG.Handlers
{
    public static class ProcessHandler
    {
        private const string LogName = "Process";

        public static int WaitDispose(Process process)
        {
            process.WaitForExit();
            var code = process.ExitCode;
            process.Dispose();
            return code;
        }

        public static void DisposeOnExit(Process process)
        {
            process.Exited += OnExit;
        }

        private static void OnExit(object sender, EventArgs e)
        {
            var proc = sender as Process;
            if (proc != null) proc.Dispose();
        }

        public static Process RunClientEXE(string filePath, string param)
        {
            return RunEXE(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),filePath), param);    
        }

        public static Process RunEXE(string filePath, string param)
        {
            if (Settings.OS != Settings.OSType.Windows)
                filePath = "mono " + filePath;

            return Run(filePath, param);
        }

        public static Process Run(string filePath, string param)
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
                    return process;
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to run process");
                Log.Error(LogName, ex);
            }

            return null;
        }

        public static Process ImpersonateClientEXEHandle(string filePath, string param, string user)
        {

            var fileName = "su";
            var arguments = string.Format(" - {0} -c {1} {2}", 
                user, 
                "mono " + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filePath),
                param);

            return Run(fileName, arguments);
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

            WaitDispose(Run("pkill", "-f " + name));
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

            WaitDispose(Run("pgrep " + name + " | while read -r line; do kill $line; exit; done", ""));
        }
    }
}
