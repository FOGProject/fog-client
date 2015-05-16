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
using System.Reflection;
using System.Runtime.InteropServices;

namespace FOG.Handlers
{
    /// <summary>
    ///     Handle all shutdown requests
    ///     The windows shutdown command is used instead of the win32 api because it notifies the user prior
    /// </summary>
    public static class ShutdownHandler
    {
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
            ForceIfHung = 0x10
        }

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

        private const string LogName = "ShutdownHandler";
        //Define variables

        public static bool ShutdownPending { get; private set; }
        public static bool UpdatePending { get; set; }
        //Load the ability to lock the computer from the native user32 dll
        [DllImport("user32")]
        private static extern void lockWorkStation();

        /// <summary>
        ///     Create a shutdown command
        /// </summary>
        /// <param name="parameters">The parameters to use</param>
        private static void CreateShutdownCommand(string parameters)
        {
            LogHandler.Log(LogName, "Creating shutdown request");
            LogHandler.Log(LogName, string.Format("Parameters: {0}", parameters));

            Process.Start("shutdown", parameters);
        }

        /// <summary>
        ///     Shutdown the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        public static void Shutdown(string comment, int seconds)
        {
            ShutdownPending = true;
            CreateShutdownCommand(string.Format("/s /c \"{0}\" /t {1}", comment, seconds));
        }

        /// <summary>
        ///     Restart the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        public static void Restart(string comment, int seconds)
        {
            ShutdownPending = true;
            CreateShutdownCommand(string.Format("/r /c \"{0}\" /t {1}", comment, seconds));
        }

        /// <summary>
        ///     Log off the current user
        /// </summary>
        public static void LogOffUser()
        {
            CreateShutdownCommand("/l");
        }

        /// <summary>
        ///     Hibernate the computer
        /// </summary>
        public static void Hibernate()
        {
            CreateShutdownCommand("/h");
        }

        /// <summary>
        ///     Lock the workstation
        /// </summary>
        public static void LockWorkStation()
        {
            lockWorkStation();
        }

        /// <summary>
        ///     Abort a shutdown if it is not to late
        /// </summary>
        public static void AbortShutdown()
        {
            ShutdownPending = false;
            CreateShutdownCommand("/a");
        }

        /// <summary>
        ///     Restart the service
        /// </summary>
        public static void RestartService()
        {
            LogHandler.Log(LogName, "Restarting service");
            ShutdownPending = true;
            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                               @"\RestartFOGService.exe"
                }
            };
            process.Start();
        }

        /// <summary>
        ///     Spawn an update waiter
        /// </summary>
        /// <param name="fileName">The file that the update waiter should spawn once the update is complete</param>
        public static void SpawnUpdateWaiter(string fileName)
        {
            LogHandler.Log(LogName, "Spawning update waiter");

            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                               @"\tmp\FOGUpdateWaiter.exe",
                    Arguments = string.Format("\"{0}\"", fileName)
                }
            };

            LogHandler.Log(LogName, "Update Waiter args");
            LogHandler.Log(LogName, string.Format("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments));
            process.Start();
        }
    }
}