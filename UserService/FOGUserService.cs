using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace FOG
{
	
    /// <summary>
    /// Coordinate all user specific FOG modules
    /// </summary>	
    class FOGUserService
    {
		
        //Define variables
        private static Thread threadManager;
        private static List<AbstractModule> modules;
        private static Thread notificationPipeThread;
        private static PipeServer notificationPipe;
        private static PipeClient servicePipe;
        private const String LOG_NAME = "UserService";
        private static int sleepDefaultTime = 60;
        private static Status status;
		
		
        public static void Main(string[] args)
        { 
            //Initialize everything
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
			
            LogHandler.setFilePath(Environment.ExpandEnvironmentVariables("%userprofile%") + @"\fog_user.log");
            LogHandler.Log(LOG_NAME, "Initializing");
            if (CommunicationHandler.GetAndSetServerAddress())
            {
	
                initializeModules();
                threadManager = new Thread(new ThreadStart(serviceLooper));
                status = Status.Stopped;
				
                //Setup the notification pipe server
                notificationPipeThread = new Thread(new ThreadStart(notificationPipeHandler));
                notificationPipe = new PipeServer("fog_pipe_notification_user_" + UserHandler.GetCurrentUser());
                notificationPipe.MessageReceived += new PipeServer.MessageReceivedHandler(pipeServer_MessageReceived);			
                notificationPipe.start();
				
                //Setup the service pipe client
                servicePipe = new PipeClient("fog_pipe_service");
                servicePipe.MessageReceived += new PipeClient.MessageReceivedHandler(pipeClient_MessageReceived);
                servicePipe.connect();
				
				
                status = Status.Running;
				
				

				
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info"))
                {
                    LogHandler.Log(LOG_NAME, "Update.info found, exiting program");
                    ShutdownHandler.SpawnUpdateWaiter(System.Reflection.Assembly.GetExecutingAssembly().Location);
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

        //Module status -- used for stopping/starting
        public enum Status
        {
            Running = 1,
            Stopped = 0
        }
		
        //This is run by the pipe thread, it will send out notifications to the tray
        private static void notificationPipeHandler()
        {
            while (true)
            {
                if (!notificationPipe.isRunning())
                    notificationPipe.start();			
				
				
                if (NotificationHandler.GetNotifications().Count > 0)
                {
                    //Split up the notification into 3 messages: Title, Message, and Duration
                    notificationPipe.sendMessage("TLE:" + NotificationHandler.GetNotifications()[0].getTitle());
                    Thread.Sleep(750);
                    notificationPipe.sendMessage("MSG:" + NotificationHandler.GetNotifications()[0].getMessage());
                    Thread.Sleep(750);
                    notificationPipe.sendMessage("DUR:" + NotificationHandler.GetNotifications()[0].getDuration().ToString());
                    NotificationHandler.RemoveNotification(0);
                } 
				
                Thread.Sleep(3000);
            }

        }
		
        //Handle recieving a message
        private static void pipeServer_MessageReceived(Client client, String message)
        {
            LogHandler.Log(LOG_NAME, "Message recieved from tray");
            LogHandler.Log(LOG_NAME, "MSG:" + message);
        }
		
        //Handle recieving a message
        private static void pipeClient_MessageReceived(String message)
        {
            LogHandler.Log(LOG_NAME, "Message recieved from service");
            LogHandler.Log(LOG_NAME, "MSG: " + message);
			
            if (message.Equals("UPD"))
            {
                ShutdownHandler.SpawnUpdateWaiter(System.Reflection.Assembly.GetExecutingAssembly().Location);
                ShutdownHandler.ScheduleUpdate();
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
            while (status.Equals(Status.Running) && !ShutdownHandler.IsShutdownPending() && !ShutdownHandler.IsUpdatePending())
            {
                foreach (AbstractModule module in modules)
                {
                    if (ShutdownHandler.IsShutdownPending() || ShutdownHandler.IsUpdatePending())
                        break;
					
                    //Log file formatting
                    LogHandler.NewLine();
                    LogHandler.NewLine();
                    LogHandler.Divider();
					
                    try
                    {
                        module.start();
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Log(LOG_NAME, "Failed to start " + module.getName());
                        LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
                    }
					
                    //Log file formatting
                    LogHandler.Divider();
                    LogHandler.NewLine();
                }
					
                if (ShutdownHandler.IsShutdownPending() || ShutdownHandler.IsUpdatePending())
                    break;				
                //Once all modules have been run, sleep for the set time
                int sleepTime = getSleepTime();
                LogHandler.Log(LOG_NAME, "Sleeping for " + sleepTime.ToString() + " seconds");
                Thread.Sleep(sleepTime * 1000);
            }
        }
		
		
        //Get the time to sleep from the FOG server, if it cannot it will use the default time
        private static int getSleepTime()
        {
            LogHandler.Log(LOG_NAME, "Getting sleep duration...");
			
            var sleepResponse = CommunicationHandler.GetResponse("/service/servicemodule-active.php");
			
            try
            {
                if (!sleepResponse.wasError() && !sleepResponse.getField("#sleep").Equals(""))
                {
                    int sleepTime = int.Parse(sleepResponse.getField("#sleep"));
                    if (sleepTime >= sleepDefaultTime)
                    {
                        return sleepTime;
                    }
                    else
                    {
                        LogHandler.Log(LOG_NAME, "Sleep time set on the server is below the minimum of " + sleepDefaultTime.ToString());
                    }
                }
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
            process.StartInfo.FileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\FOGTray.exe";
            process.Start();
        }
		
        static void OnProcessExit(object sender, EventArgs e)
        {
			
        }

    }
}