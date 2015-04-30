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
using System.Reflection;
using System.Threading;
using FOG.Handlers;
using FOG.Modules;

namespace FOG
{
    /// <summary>
    ///     Coordinate all user specific FOG modules
    /// </summary>
    internal class FOGUserService
    {
        //Module status -- used for stopping/starting
        public enum Status
        {
            Running = 1,
            Stopped = 0
        }

        private const string LOG_NAME = "UserService";
        //Define variables
        private static Thread threadManager;
        private static List<AbstractModule> modules;
        private static Thread notificationPipeThread;
        private static PipeServer notificationPipe;
        private static PipeClient servicePipe;
        private static readonly int sleepDefaultTime = 60;
        private static Status status;

        public static void Main(string[] args)
        {
            //Initialize everything
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            LogHandler.FilePath = (Environment.ExpandEnvironmentVariables("%userprofile%") + @"\fog_user.log");
            LogHandler.Log(LOG_NAME, "Initializing");
            if (CommunicationHandler.GetAndSetServerAddress())
            {
                initializeModules();
                threadManager = new Thread(serviceLooper);
                status = Status.Stopped;

                //Setup the notification pipe server
                notificationPipeThread = new Thread(notificationPipeHandler);
                notificationPipe = new PipeServer("fog_pipe_notification_user_" + UserHandler.GetCurrentUser());
                notificationPipe.MessageReceived += pipeServer_MessageReceived;
                notificationPipe.start();

                //Setup the service pipe client
                servicePipe = new PipeClient("fog_pipe_service");
                servicePipe.MessageReceived += pipeClient_MessageReceived;
                servicePipe.connect();


                status = Status.Running;


                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info"))
                {
                    LogHandler.Log(LOG_NAME, "Update.info found, exiting program");
                    ShutdownHandler.SpawnUpdateWaiter(Assembly.GetExecutingAssembly().Location);
                    Environment.Exit(0);
                }


                //Start the main thread that handles all modules
                threadManager.Priority = ThreadPriority.Normal;
                threadManager.IsBackground = false;
                threadManager.Start();

                if (RegistryHandler.GetSystemSetting("Tray").Trim().Equals("1"))
                {
                    startTray();
                }
            }
        }

        //This is run by the pipe thread, it will send out notifications to the tray
        private static void notificationPipeHandler()
        {
            while (true)
            {
                if (!notificationPipe.isRunning())
                    notificationPipe.start();


                if (NotificationHandler.Notifications.Count > 0)
                {
                    //Split up the notification into 3 messages: Title, Message, and Duration
                    notificationPipe.sendMessage("TLE:" + NotificationHandler.Notifications[0].Title);
                    Thread.Sleep(750);
                    notificationPipe.sendMessage("MSG:" + NotificationHandler.Notifications[0].Message);
                    Thread.Sleep(750);
                    notificationPipe.sendMessage("DUR:" + NotificationHandler.Notifications[0].Duration);
                    NotificationHandler.Notifications.RemoveAt(0);
                }

                Thread.Sleep(3000);
            }
        }

        //Handle recieving a message
        private static void pipeServer_MessageReceived(Client client, string message)
        {
            LogHandler.Log(LOG_NAME, "Message recieved from tray");
            LogHandler.Log(LOG_NAME, "MSG:" + message);
        }

        //Handle recieving a message
        private static void pipeClient_MessageReceived(string message)
        {
            LogHandler.Log(LOG_NAME, "Message recieved from service");
            LogHandler.Log(LOG_NAME, "MSG: " + message);

            if (message.Equals("UPD"))
            {
                ShutdownHandler.SpawnUpdateWaiter(Assembly.GetExecutingAssembly().Location);
                ShutdownHandler.UpdatePending = true;
            }
        }

        //Load all of the modules
        private static void initializeModules()
        {
            modules = new List<AbstractModule>();
            modules.Add(new AutoLogOut());
            modules.Add(new DisplayManager());
        }

        //Run each service
        private static void serviceLooper()
        {
            //Only run the service if there wasn't a stop or shutdown request
            while (status.Equals(Status.Running) && !ShutdownHandler.ShutdownPending && !ShutdownHandler.UpdatePending)
            {
                foreach (var module in modules)
                {
                    if (ShutdownHandler.ShutdownPending || ShutdownHandler.UpdatePending)
                        break;

                    LogHandler.NewLine();
                    LogHandler.PaddedHeader(module.Name);
                    LogHandler.Log("Client-Info", "Version: " + RegistryHandler.GetSystemSetting("Version"));

                    try
                    {
                        module.start();
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Log(LOG_NAME, "Failed to start " + module.Name);
                        LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
                    }

                    //Log file formatting
                    LogHandler.Divider();
                    LogHandler.NewLine();
                }

                if (ShutdownHandler.ShutdownPending || ShutdownHandler.UpdatePending)
                    break;
                //Once all modules have been run, sleep for the set time
                var sleepTime = getSleepTime();
                LogHandler.Log(LOG_NAME, "Sleeping for " + sleepTime + " seconds");
                Thread.Sleep(sleepTime*1000);
            }
        }

        //Get the time to sleep from the FOG server, if it cannot it will use the default time
        private static int getSleepTime()
        {
            LogHandler.Log(LOG_NAME, "Getting sleep duration...");
            try
            {
                var sleepTimeStr = RegistryHandler.GetSystemSetting("Sleep");
                var sleepTime = int.Parse(sleepTimeStr);
                if (sleepTime >= sleepDefaultTime)
                {
                    return sleepTime;
                }
                LogHandler.Log(LOG_NAME, "Sleep time set on the server is below the minimum of " + sleepDefaultTime);
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Failed to parse sleep time");
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }

            LogHandler.Log(LOG_NAME, "Using default sleep time");

            return sleepDefaultTime;
        }

        private static void startTray()
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                         @"\FOGTray.exe";
            process.Start();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
        }
    }
}