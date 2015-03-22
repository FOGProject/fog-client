using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

using FOG;

namespace FOG
{
    /// <summary>
    /// Handle all shutdown requests
    /// The windows shutdown command is used instead of the win32 api because it notifies the user prior
    /// </summary>
    public static class ShutdownHandler
    {

        //Define variables
        public static Boolean ShutdownPending { get; private set; }
        public static Boolean UpdatePending { get; set; }
        private const String LOG_NAME = "ShutdownHandler";
		
        //Load the ability to lock the computer from the native user32 dll
        [DllImport("user32")]
        private static extern void lockWorkStation();
		
        //List all possible shutdown types
        public enum ShutDownType
        {
            LogOff = 0,
            Shutdown = 1,
            Reboot = 2,
            ForcedLogOff = 4,
            ForcedShutdown = 5,
            ForcedReboot = 6,
            PowerOff = 8,
            ForcedPowerOff = 12
        }
		
        //List options on how to exit windows
        [Flags]
        public enum ExitWindows : uint
        {
            LogOff = 0x00,
            ShutDown = 0x01,
            Reboot = 0x02,
            PowerOff = 0x08,
            RestartApps = 0x40,
            Force = 0x04,
            ForceIfHung = 0x10,
        }
		
        /// <summary>
        /// Create a shutdown command
        /// </summary>
        /// <param name="parameters">The parameters to use</param>
        private static void createShutdownCommand(String parameters)
        {
            LogHandler.Log(LOG_NAME, "Creating shutdown request");
            LogHandler.Log(LOG_NAME, "Parameters: " + parameters);

            Process.Start("shutdown", parameters);
        }
        
        /// <summary>
        /// Shutdown the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="seconds">How long to wait before processing the request</param>		
        public static void Shutdown(String comment, int seconds)
        {
            ShutdownPending = true;
            createShutdownCommand("/s /c \"" + comment + "\" /t " + seconds);
        }
		
        /// <summary>
        /// Restart the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        public static void Restart(String comment, int seconds)
        {
            ShutdownPending = true;
            createShutdownCommand("/r /c \"" + comment + "\" /t " + seconds);
        }
		
        /// <summary>
        /// Log off the current user
        /// </summary>
        public static void LogOffUser()
        {
            createShutdownCommand("/l");
        }
		
        /// <summary>
        /// Hibernate the computer
        /// </summary>
        public static void Hibernate()
        {
            createShutdownCommand("/h");
        }
		
        /// <summary>
        /// Lock the workstation
        /// </summary>
        public static void LockWorkStation()
        {			
            lockWorkStation();
        }
		
        /// <summary>
        /// Abort a shutdown if it is not to late
        /// </summary>
        public static void AbortShutdown()
        {		
            ShutdownPending = false;
            createShutdownCommand("/a");
        }
		
        /// <summary>
        /// Restart the service
        /// </summary>
        public static void RestartService()
        {
            LogHandler.Log(LOG_NAME, "Restarting service");
            ShutdownPending = true;
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\RestartFOGService.exe";
            process.Start();
        }
		
        /// <summary>
        /// Spawn an update waiter
        /// </summary>
        /// <param name="fileName">The file that the update waiter should spawn once the update is complete</param>
        public static void SpawnUpdateWaiter(String fileName)
        {
            LogHandler.Log(LOG_NAME, "Spawning update waiter");
			
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\tmp\FOGUpdateWaiter.exe";
            process.StartInfo.Arguments = "\"" + fileName + "\"";
			
            LogHandler.Log(LOG_NAME, "Update Waiter args");
            LogHandler.Log(LOG_NAME, process.StartInfo.FileName + " " + process.StartInfo.Arguments);
            process.Start();			
        }
    }
}