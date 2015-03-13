using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;

using FOG;

namespace FOG{
	/// <summary>
	/// Coordinate all system wide FOG modules
	/// </summary>
	public partial class FOGService  : ServiceBase {
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
		public enum Status {
			Broken = 2,
			Running = 1,
			Stopped = 0
		}
		
		public FOGService() {
			//Initialize everything
			if(CommunicationHandler.GetAndSetServerAddress()) {

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
				ShutdownHandler.UnScheduleUpdate();
			}
		}
		
		//This is run by the pipe thread, it will send out notifications to the tray
		private void notificationPipeHandler() {
			while (true) {
				if(!this.notificationPipe.isRunning()) 
					this.notificationPipe.start();			
				
				if(NotificationHandler.GetNotifications().Count > 0) {
					//Split up the notification into 3 messages: Title, Message, and Duration
					this.notificationPipe.sendMessage("TLE:" + NotificationHandler.GetNotifications()[0].getTitle());
					Thread.Sleep(750);
					this.notificationPipe.sendMessage("MSG:" + NotificationHandler.GetNotifications()[0].getMessage());
					Thread.Sleep(750);
					this.notificationPipe.sendMessage("DUR:" + NotificationHandler.GetNotifications()[0].getDuration().ToString());
					NotificationHandler.RemoveNotification(0);
				} 
				
				Thread.Sleep(3000);
			}
		}		
		
		//Handle recieving a message
		private void notificationPipeServer_MessageReceived(Client client, String message) {
			LogHandler.Log("PipeServer", "Notification message recieved");
			LogHandler.Log("PipeServer",message);
		}

		//Handle recieving a message
		private void servicePipeService_MessageReceived(Client client, String message) {
			LogHandler.Log("PipeServer", "Server-Pipe message recieved");
			LogHandler.Log("PipeServer",message);
		}		

		//Called when the service starts
		protected override void OnStart(string[] args) {
			if(!this.status.Equals(Status.Broken)) {
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
				ShutdownHandler.UnScheduleUpdate();
			}
        }
		
		//Load all of the modules
		private void initializeModules() {
			this.modules = new List<AbstractModule>();
			this.modules.Add(new ClientUpdater());
			this.modules.Add(new TaskReboot());
			this.modules.Add(new HostnameChanger());
			this.modules.Add(new SnapinClient());
			this.modules.Add(new DisplayManager());			
			this.modules.Add(new HostRegister());
			this.modules.Add(new GreenFOG());
			this.modules.Add(new DirCleaner());
			this.modules.Add(new UserCleanup());
		}
		
		//Called when the service stops
		protected override void OnStop() {
			if(!this.status.Equals(Status.Broken))
				this.status = Status.Stopped;
		}
		
		//Run each service
		private void serviceLooper() {
			CommunicationHandler.Authenticate();
			LogHandler.NewLine();
			//Only run the service if there wasn't a stop or shutdown request
			while (status.Equals(Status.Running) && !ShutdownHandler.IsShutdownPending() && !ShutdownHandler.IsUpdatePending()) {
				foreach(AbstractModule module in modules) {
					if(ShutdownHandler.IsShutdownPending() || ShutdownHandler.IsUpdatePending())
						break;
					
					//Log file formatting
					LogHandler.NewLine();
					LogHandler.Divider();
					
					try {
						module.start();
					} catch (Exception ex) {
						LogHandler.Log(LOG_NAME, "Failed to start " + module.getName());
						LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
					}
					
					//Log file formatting
					LogHandler.Divider();
					LogHandler.NewLine();
				}
				
				
				if(!ShutdownHandler.IsShutdownPending() && !ShutdownHandler.IsUpdatePending()) {
					//Once all modules have been run, sleep for the set time
					int sleepTime = getSleepTime();
					LogHandler.Log(LOG_NAME, "Sleeping for " + sleepTime.ToString() + " seconds");
					Thread.Sleep(sleepTime * 1000);
				}
			}
			
			if(ShutdownHandler.IsUpdatePending()) {
				UpdateHandler.beginUpdate(servicePipe);
			}
		}

		//Get the time to sleep from the FOG server, if it cannot it will use the default time
		private int getSleepTime() {
			LogHandler.Log(LOG_NAME, "Getting sleep duration...");
			
			Response sleepResponse = CommunicationHandler.GetResponse("/management/index.php?node=client&sub=configure");
			
			try {
				if(!sleepResponse.wasError() && !sleepResponse.getField("#sleep").Equals("")) {
					int sleepTime = int.Parse(sleepResponse.getField("#sleep"));
					if(sleepTime >= this.sleepDefaultTime) {
						return sleepTime;
					} else {
						LogHandler.Log(LOG_NAME, "Sleep time set on the server is below the minimum of " + this.sleepDefaultTime.ToString());
					}
				}
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME,"Failed to parse sleep time");
				LogHandler.Log(LOG_NAME,"ERROR: " + ex.Message);				
			}
			
			LogHandler.Log(LOG_NAME,"Using default sleep time");	
			
			return this.sleepDefaultTime;			
		} 

	}
}
