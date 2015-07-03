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
using Newtonsoft.Json.Linq;

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
        public static bool Requested { get; private set; }
        public static bool Updating { get; set; }

        // Variables needed for aborting a shutdown
        private static Timer _timer;
        private static bool delayed;
        private const int DefaultGracePeriod = 60;
        private static dynamic requestData = new JObject();

        public enum FormOption
        {
            None,
            Abort,
            Delay
        }


        //Load the ability to lock the computer from the native user32 dll
        [DllImport("user32")]
        private static extern void lockWorkStation();

        static Power()
        {
            Bus.Subscribe(Bus.Channel.Power, ParseBus);
        }

        private static void ParseBus(dynamic data)
        {
            if (data.action == null) return;
            string action = data.action.ToString();
            action = action.Trim();

            if (action.Equals("abort"))
                AbortShutdown();
            else if (action.Equals("shuttingdown"))
                ShuttingDown = true;
            else if (action.Equals("help"))
                HelpShutdown(data);
            else if (action.Equals("delay"))
                DelayShutdown(data);
            else if (action.Equals("request"))
                Requested = true;
        }

        private static void DelayShutdown(dynamic data)
        {
            if (_timer != null) 
            {
                _timer.Stop();
                _timer.Dispose();
            }

            if (data.delay == null) return;

            //DelayTime is in minutes
            var delayTime = -1;

            try
            {
                delayTime = (int) data.delay;
            }
            catch (Exception)
            {
                return;
            }

            if (delayTime < 1)
                return;

            Log.Entry(LogName, "Delayed power action by " + delayTime + " minutes");
            delayed = true;
            _timer = new Timer(delayTime*1000*60);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private static void HelpShutdown(dynamic data)
        {
            if (data.type == null) return;
            string type = data.type.ToString();
            type = type.Trim();

            if(type.Equals("shutdown"))
                Shutdown(data.reason.ToString() ?? "");
            else if(type.Equals("reboot"))
                Restart(data.reason.ToString() ?? "");
        }

        /// <summary>
        ///     Create a shutdown command
        /// </summary>
        /// <param name="parameters">The parameters to use</param>
        private static void CreateTask(string parameters)
        {
            requestData = new JObject();

            Log.Entry(LogName, "Creating shutdown request");
            Log.Entry(LogName, string.Format("Parameters: {0}", parameters));

            Process.Start("shutdown", parameters);
        }

        private static void QueueShutdown(string parameters, FormOption options = FormOption.Abort, string message = null, int gracePeriod = -1)
        {
            if (_timer != null && _timer.Enabled)
            {
                Log.Entry(LogName, "Power task already in-progress");
                return;
            }

            delayed = false;

            try
            {
                if (gracePeriod == -1)
                    gracePeriod = (!string.IsNullOrEmpty(RegistryHandler.GetSystemSetting("gracePeriod")))
                        ? int.Parse(RegistryHandler.GetSystemSetting("gracePeriod"))
                        : DefaultGracePeriod;
            }
            catch (Exception)
            {
                gracePeriod = DefaultGracePeriod;
            }

            Log.Entry(LogName, string.Format("Creating shutdown command in {0} seconds", gracePeriod));
            
            requestData = new JObject();
            requestData.action = "request";
            requestData.period = gracePeriod;
            requestData.options = options;
            requestData.command = parameters;
            requestData.message = message ?? "This computer needs to perform maintance.";

            Bus.Emit(Bus.Channel.Power, requestData, true);
            _timer = new Timer(gracePeriod*1000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (delayed)
                {
                    _timer.Dispose();

                    string message = null;

                    if (requestData.message != null)
                        message = requestData.message.ToString();

                    QueueShutdown(requestData.command.ToString(), FormOption.None, message, (int)requestData.period);
                    return;
                }

                ShuttingDown = true;
                Requested = false;
                CreateTask(requestData.command.ToString());

                dynamic json = new JObject();
                json.action = "shuttingdown";
                Bus.Emit(Bus.Channel.Power, json, true);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not create shutdown command from request");
                Log.Error(LogName, ex);
            }
        }

        /// <summary>
        ///     Shutdown the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="options">The options the user has on the prompt form</param>
        /// <param name="message">The message to show in the shutdown gui</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        public static void Shutdown(string comment, FormOption options = FormOption.Abort, string message = null, int seconds = 30)
        {
            QueueShutdown(string.Format("/s /c \"{0}\" /t {1}", comment, seconds), options, message);
        }

        /// <summary>
        ///     Restart the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="options">The options the user has on the prompt form</param>
        /// <param name="message">The message to show in the shutdown gui</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        public static void Restart(string comment, FormOption options = FormOption.Abort, string message = null, int seconds = -1)
        {
            QueueShutdown(string.Format("/r /c \"{0}\" /t {1}", comment, seconds), options, message);
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
            Log.Entry(LogName, "Aborting shutdown");
            ShuttingDown = false;
            _timer.Stop();
            _timer.Close();
            _timer = null;
            Requested = false;
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