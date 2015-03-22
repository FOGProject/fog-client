/**
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2015 FOG Project
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
**/

using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Diagnostics;

using FOG;

namespace FOG
{
    /// <summary>
    /// Coordinate all system wide FOG modules
    /// </summary>
    public partial class FOGService  : ServiceBase
    {
        //Define variables
        private Thread threadManager;
        private Thread notificationPipeThread;
		
        private List<AbstractModule> modules;
        private Status status;
        private int sleepDefaultTime = 60;
        private PipeServer notificationPipe;
        private PipeServer servicePipe;
		
        private const String LOG_NAME = "Service";
		
        //Module status -- used for stopping/starting
        public enum Status
        {
            Broken = 2,
            Running = 1,
            Stopped = 0
        }
		
        public FOGService()
        {
            //Initialize everything
            if (CommunicationHandler.GetAndSetServerAddress())
            {

                initializeModules();
                this.threadManager = new Thread(new ThreadStart(serviceLooper));
                this.status = Status.Stopped;
				
                //Setup the notification pipe server
                this.notificationPipeThread = new Thread(new ThreadStart(notificationPipeHandler));
                this.notificationPipe = new PipeServer("fog_pipe_notification");
                this.notificationPipe.MessageReceived += new PipeServer.MessageReceivedHandler(notificationPipeServer_MessageReceived);
				
                //Setup the user-service pipe server, this is only Server -- > Client communication so no need to setup listeners
                this.servicePipe = new PipeServer("fog_pipe_service");
                this.servicePipe.MessageReceived += new PipeServer.MessageReceivedHandler(servicePipeService_MessageReceived);
				
                //Unschedule any old updates
                ShutdownHandler.UpdatePending = false;
            }
        }
		
        //This is run by the pipe thread, it will send out notifications to the tray
        private void notificationPipeHandler()
        {
            while (true)
            {
                if (!this.notificationPipe.isRunning())
                    this.notificationPipe.start();			
				
                if (NotificationHandler.Notifications.Count > 0)
                {
                    //Split up the notification into 3 messages: Title, Message, and Duration
                    this.notificationPipe.sendMessage("TLE:" + NotificationHandler.Notifications[0].Title);
                    Thread.Sleep(750);
                    this.notificationPipe.sendMessage("MSG:" + NotificationHandler.Notifications[0].Message);
                    Thread.Sleep(750);
                    this.notificationPipe.sendMessage("DUR:" + NotificationHandler.Notifications[0].Duration);
                    NotificationHandler.Notifications.RemoveAt(0);
                } 
				
                Thread.Sleep(3000);
            }
        }
		
        //Handle recieving a message
        private void notificationPipeServer_MessageReceived(Client client, String message)
        {
            LogHandler.Log("PipeServer", "Notification message recieved");
            LogHandler.Log("PipeServer", message);
        }

        //Handle recieving a message
        private void servicePipeService_MessageReceived(Client client, String message)
        {
            LogHandler.Log("PipeServer", "Server-Pipe message recieved");
            LogHandler.Log("PipeServer", message);
        }

        //Called when the service starts
        protected override void OnStart(string[] args)
        {
            if (!this.status.Equals(Status.Broken))
            {
                this.status = Status.Running;
				
                //Start the pipe server
                this.notificationPipeThread.Priority = ThreadPriority.Normal;
                this.notificationPipeThread.Start();
				
                this.servicePipe.start();
			
                //Start the main thread that handles all modules
                this.threadManager.Priority = ThreadPriority.Normal;
                this.threadManager.IsBackground = true;
                this.threadManager.Name = "FOGService";
                this.threadManager.Start();
				
                //Unschedule any old updates
                ShutdownHandler.UpdatePending = false;
				
                //Delete old temp files
                try
                {
                    if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\tmp"))
                    {
                        Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\tmp");
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.Log(LOG_NAME, "Could not delete tmp dir");
                    LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
                }
            }
        }
		
        //Load all of the modules
        private void initializeModules()
        {
            this.modules = new List<AbstractModule>();
            this.modules.Add(new ClientUpdater());
            this.modules.Add(new TaskReboot());
            this.modules.Add(new HostnameChanger());
            this.modules.Add(new SnapinClient());
            this.modules.Add(new DisplayManager());			
            this.modules.Add(new GreenFOG());
        }
		
        //Called when the service stops
        protected override void OnStop()
        {
            if (!this.status.Equals(Status.Broken))
                this.status = Status.Stopped;
			
            foreach (Process process in Process.GetProcessesByName("FOGUserService"))
            {
                process.Kill();
            }
            foreach (Process process in Process.GetProcessesByName("FOGTray"))
            {
                process.Kill();
            }		
            //Delete old temp files
            try
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\tmp"))
                {
                    Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\tmp");
                }
            }
            catch (Exception ex)
            {
            }
        }

		
        //Run each service
        private void serviceLooper()
        {
			
            LogHandler.NewLine();
            LogHandler.PaddedHeader("Authentication");
            LogHandler.Log("Client-Info", "Version: " + RegistryHandler.GetSystemSetting("Version"));
            CommunicationHandler.Authenticate();
            LogHandler.NewLine();
			
            //Only run the service if there wasn't a stop or shutdown request
            while (status.Equals(Status.Running) && !ShutdownHandler.ShutdownPending && !ShutdownHandler.UpdatePending)
            {
                foreach (AbstractModule module in modules)
                {
                    if (ShutdownHandler.ShutdownPending || ShutdownHandler.UpdatePending)
                        break;
					
                    //Log file formatting
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
				
				
                if (!ShutdownHandler.ShutdownPending && !ShutdownHandler.UpdatePending)
                {
                    //Once all modules have been run, sleep for the set time
                    int sleepTime = getSleepTime();
                    LogHandler.Log(LOG_NAME, "Sleeping for " + sleepTime + " seconds");
                    Thread.Sleep(sleepTime * 1000);
                }
            }
			
            if (ShutdownHandler.UpdatePending)
            {
                UpdateHandler.beginUpdate(servicePipe);
            }
        }

        //Get the time to sleep from the FOG server, if it cannot it will use the default time
        private int getSleepTime()
        {
            LogHandler.Log(LOG_NAME, "Getting sleep duration...");
			
            Response sleepResponse = CommunicationHandler.GetResponse("/management/index.php?node=client&sub=configure");
			
            try
            {
                if (!sleepResponse.Error && !sleepResponse.getField("#sleep").Equals(""))
                {
                    int sleepTime = int.Parse(sleepResponse.getField("#sleep"));
                    if (sleepTime >= this.sleepDefaultTime)
                    {
                        return sleepTime;
                    }
                    else
                    {
                        LogHandler.Log(LOG_NAME, "Sleep time set on the server is below the minimum of " + this.sleepDefaultTime.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Failed to parse sleep time");
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);				
            }
			
            LogHandler.Log(LOG_NAME, "Using default sleep time");	
			
            return this.sleepDefaultTime;			
        }

    }
}
