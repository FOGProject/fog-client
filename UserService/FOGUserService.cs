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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FOG.Handlers;
using FOG.Modules;
using FOG.Modules.AutoLogOut;
using FOG.Modules.DisplayManager;

namespace FOG
{
    /// <summary>
    ///     Coordinate all user specific FOG modules
    /// </summary>
    internal class FOGUserService
    {

        private const string LogName = "UserService";
        //Define variables
        private static Thread _threadManager;
        private static List<AbstractModule> _modules;
        private static Thread _notificationPipeThread;
        private static PipeServer _notificationPipe;
        private static PipeClient _servicePipe;
        private const int SleepDefaultTime = 60;

        public static void Main(string[] args)
        {
            //Initialize everything
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            LogHandler.FilePath = (Environment.ExpandEnvironmentVariables("%userprofile%") + @"\fog_user.log");
            AppDomain.CurrentDomain.UnhandledException += LogHandler.UnhandledException;

            LogHandler.Log(LogName, "Initializing");
            if (!CommunicationHandler.GetAndSetServerAddress()) return;
            InitializeModules();
            _threadManager = new Thread(ServiceLooper);

            //Setup the notification pipe server
            _notificationPipeThread = new Thread(notificationPipeHandler);
            _notificationPipe = new PipeServer("fog_pipe_notification_user_" + UserHandler.GetCurrentUser());
            _notificationPipe.MessageReceived += pipeServer_MessageReceived;
            _notificationPipe.Start();

            //Setup the service pipe client
            _servicePipe = new PipeClient("fog_pipe_service");
            _servicePipe.MessageReceived += pipeClient_MessageReceived;
            _servicePipe.Connect();

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info"))
            {
                LogHandler.Log(LogName, "Update.info found, exiting program");
                ShutdownHandler.SpawnUpdateWaiter(Assembly.GetExecutingAssembly().Location);
                Environment.Exit(0);
            }


            //Start the main thread that handles all modules
            _threadManager.Priority = ThreadPriority.Normal;
            _threadManager.IsBackground = false;
            _threadManager.Start();

            if (RegistryHandler.GetSystemSetting("Tray").Equals("1"))
                StartTray();
        }

        //This is run by the pipe thread, it will send out notifications to the tray
        private static void notificationPipeHandler()
        {
            while (!ShutdownHandler.ShutdownPending && !ShutdownHandler.UpdatePending)
            {
                if (!_notificationPipe.IsRunning())
                    _notificationPipe.Start();


                if (NotificationHandler.Notifications.Count > 0)
                {
                    //Split up the notification into 3 messages: Title, Message, and Duration
                    _notificationPipe.SendMessage(string.Format("TLE:{0}", NotificationHandler.Notifications[0].Title));
                    Thread.Sleep(750);
                    _notificationPipe.SendMessage(string.Format("MSG:{0}", NotificationHandler.Notifications[0].Message));
                    Thread.Sleep(750);
                    _notificationPipe.SendMessage(string.Format("DUR:{0}", NotificationHandler.Notifications[0].Duration));
                    NotificationHandler.Notifications.RemoveAt(0);
                }

                Thread.Sleep(3000);
            }
        }

        //Handle recieving a message
        private static void pipeServer_MessageReceived(Client client, string message)
        {
            LogHandler.Debug(LogName, "Message recieved from tray");
            LogHandler.Debug(LogName, string.Format("MSG:{0}", message));
        }

        //Handle recieving a message
        private static void pipeClient_MessageReceived(string message)
        {
            LogHandler.Debug(LogName, "Message recieved from service");
            LogHandler.Debug(LogName, string.Format("MSG: {0}", message));

            if (!message.Equals("UPD")) return;
            ShutdownHandler.SpawnUpdateWaiter(Assembly.GetExecutingAssembly().Location);
            ShutdownHandler.UpdatePending = true;
        }

        //Load all of the modules
        private static void InitializeModules()
        {
            _modules = new List<AbstractModule> {new AutoLogOut(), new DisplayManager()};
        }

        //Run each service
        private static void ServiceLooper()
        {
            //Only run the service if there wasn't a stop or shutdown request
            while (!ShutdownHandler.ShutdownPending && !ShutdownHandler.UpdatePending)
            {
                foreach (var module in _modules.TakeWhile(module => !ShutdownHandler.ShutdownPending && !ShutdownHandler.UpdatePending))
                {
                    LogHandler.NewLine();
                    LogHandler.PaddedHeader(module.Name);
                    LogHandler.Log("Client-Info", string.Format("Version: {0}", RegistryHandler.GetSystemSetting("Version")));

                    try
                    {
                        module.Start();
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Error(LogName, ex);
                    }

                    //Log file formatting
                    LogHandler.Divider();
                    LogHandler.NewLine();
                }

                if (ShutdownHandler.ShutdownPending || ShutdownHandler.UpdatePending)
                    break;
                //Once all modules have been run, sleep for the set time
                var sleepTime = GetSleepTime();
                LogHandler.Log(LogName, string.Format("Sleeping for {0} seconds", sleepTime));
                Thread.Sleep(sleepTime*1000);
            }
        }

        //Get the time to sleep from the FOG server, if it cannot it will use the default time
        private static int GetSleepTime()
        {
            LogHandler.Log(LogName, "Getting sleep duration...");
            try
            {
                var sleepTimeStr = RegistryHandler.GetSystemSetting("Sleep");
                var sleepTime = int.Parse(sleepTimeStr);
                if (sleepTime >= SleepDefaultTime)
                    return sleepTime;
                LogHandler.Log(LogName, string.Format("Sleep time set on the server is below the minimum of {0}", SleepDefaultTime));
            }
            catch (Exception ex)
            {
                LogHandler.Error(LogName, ex);
            }

            LogHandler.Log(LogName, "Using default sleep time");

            return SleepDefaultTime;
        }

        private static void StartTray()
        {
            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = string.Format("{0}\\FOGTray.exe", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                }
            };
            process.Start();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
        }
    }
}