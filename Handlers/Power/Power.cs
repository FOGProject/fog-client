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
using System.Timers;

namespace FOG.Handlers.Power
{
    /// <summary>
    ///     Handle all shutdown requests
    ///     The windows shutdown command is used instead of the win32 api because it notifies the user prior
    /// </summary>
    public static class Power
    {
        private const string LogName = "Power";
        public static bool ShuttingDown { get; private set; }
        public static bool Updating { get; set; }

        // Variables needed for aborting a shutdown
        private static Timer _timer;
        private static string pendingCommand = string.Empty;
        private const int DefaultGracePeriod = 120;
        private static bool _intilized = Initialize();
        private static Process _notificationProcess;


        //Load the ability to lock the computer from the native user32 dll
        [DllImport("user32")]
        private static extern void lockWorkStation();

        private static bool Initialize()
        {
            Bus.Subscribe(Bus.Channel.Power, ParseBus);
            return true;
        }

        private static void ParseBus(string data)
        {
            if (data.Equals("AbortShutdown"))
                AbortShutdown();
            else if (data.Equals("ShuttingDown"))
                ShuttingDown = true;
            else if (data.Equals("ShutdownRequested"))
                ShutdownNotification();
        }

        /// <summary>
        ///     Create a shutdown command
        /// </summary>
        /// <param name="parameters">The parameters to use</param>
        private static void CreateTask(string parameters)
        {
            Log.Entry(LogName, "Creating shutdown request");
            Log.Entry(LogName, string.Format("Parameters: {0}", parameters));

            Process.Start("shutdown", parameters);
        }

        private static void QueueShutdown(string parameters, int gracePeriod = DefaultGracePeriod)
        {
            Log.Entry(LogName, string.Format("Creating shutdown command in {0} seconds", gracePeriod*1000));
            pendingCommand = parameters;
            _timer = new Timer(gracePeriod*1000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            ShuttingDown = true;
            CreateTask(pendingCommand);
            pendingCommand = string.Empty;

            Bus.Emit(Bus.Channel.Power, "ShuttingDown");
        }

        /// <summary>
        ///     Shutdown the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        public static void Shutdown(string comment, int seconds)
        {
            QueueShutdown(string.Format("/s /c \"{0}\" /t {1}", comment, seconds));
        }

        /// <summary>
        ///     Restart the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        public static void Restart(string comment, int seconds)
        {
            QueueShutdown(string.Format("/r /c \"{0}\" /t {1}", comment, seconds));
        }

        /// <summary>
        ///     Entry off the current user
        /// </summary>
        public static void LogOffUser()
        {
            CreateTask("/l");
        }

        /// <summary>
        ///     Hibernate the computer
        /// </summary>
        public static void Hibernate()
        {
            CreateTask("/h");
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
            ShuttingDown = false;
            _timer = null;
        }

        private static void ShutdownNotification()
        {
            Log.Entry(LogName, "Prompting user");
            _notificationProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                               @"\FOGNotificationGUI.exe"
                }
            };
            _notificationProcess.Start();

            _notificationProcess.Exited += ProcessNotificationGUI;
        }

        private static void ProcessNotificationGUI(object sender, EventArgs e)
        {
            if(_notificationProcess.ExitCode == 1)
                Bus.Emit(Bus.Channel.Power, "AbortShutdown", true);
        }

        /// <summary>
        ///     Restart the service
        /// </summary>
        public static void RestartService()
        {
            Log.Entry(LogName, "Restarting service");
            ShuttingDown = true;
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
            Log.Entry(LogName, "Spawning update waiter");

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

            Log.Entry(LogName, "Update Waiter args");
            Log.Entry(LogName, string.Format("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments));
            process.Start();
        }
    }
}