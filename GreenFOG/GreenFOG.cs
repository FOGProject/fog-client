
using System;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace FOG {
	/// <summary>
	/// Perform cron style power tasks
	/// </summary>
	public class GreenFOG : AbstractModule {
		
		public GreenFOG():base(){
			setName("GreenFOG");
			setDescription("Perform cron style power tasks");
		}
		
		protected override void doWork() {
			//Get actions
			Response tasksResponse = CommunicationHandler.getResponse("/service/greenfog.php?mac=" + CommunicationHandler.getMacAddresses());

			//Shutdown if a task is avaible and the user is logged out or it is forced
			if(!tasksResponse.wasError()) {
				List<String> tasks = CommunicationHandler.parseDataArray(tasksResponse, "#task", false);
				
				//Remove old actions
				RegistryHandler.deleteModule(getName());
				
				//Add tasks
				createTasks(tasks);
			}
			
		}
		
		private void createTasks(List<String> tasks) {
			TaskService taskService = new TaskService();
			
			int index = 0;
			
			foreach(String task in tasks) {
				//Split up the response
				LogHandler.log(getName(), "Adding task: " + task);
				String[] taskData = task.Split('@');
				
				LogHandler.log(getName(), "Creating task definition");
				//Create task definition
				TaskDefinition taskDefinition = taskService.NewTask();
				taskDefinition.RegistrationInfo.Description = "Green FOG";
				taskDefinition.Principal.UserId = "SYSTEM";
				
				LogHandler.log(getName(), "Creating task trigger");
				//Create task trigger
				DailyTrigger trigger = new DailyTrigger(1);
				trigger.StartBoundary = DateTime.Today + TimeSpan.FromHours(int.Parse(taskData[0])) + TimeSpan.FromMinutes(int.Parse(taskData[1])); //Run at the specified time
				
				taskDefinition.Triggers.Add(trigger);
				
				LogHandler.log(getName(), "Creating task action");
				//Create task action
				if(taskData[2].Equals("r")) {
					taskDefinition.Actions.Add(new ExecAction("shutdown.exe", "/r /c \"Green FOG\" /t 0", null));
				} else if(taskData[2].Equals("s")) {
					taskDefinition.Actions.Add(new ExecAction("shutdown.exe", "/s /c \"Green FOG\" /t 0", null));
				}
				
				LogHandler.log(getName(), "Registering task definition");
				//Register the task
				try {
					taskService.RootFolder.RegisterTaskDefinition(@"FOG\GreenFOG_ " + index.ToString(), taskDefinition);
				} catch (Exception ex) {
					LogHandler.log(getName(), "Error registering tasK");
					LogHandler.log(getName(), "ERROR: " + ex.Message);
				}
				index++;
			}
			
		}
	}
}