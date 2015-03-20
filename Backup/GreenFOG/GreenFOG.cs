
using System;
using System.Linq;
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
			Response tasksResponse = CommunicationHandler.GetResponse("/service/greenfog.php?mac=" + CommunicationHandler.GetMacAddresses());

			//Shutdown if a task is avaible and the user is logged out or it is forced
			if(!tasksResponse.wasError()) {
				List<String> tasks = CommunicationHandler.ParseDataArray(tasksResponse, "#task", false);
				
				//Filter existing tasks
				tasks = filterTasks(tasks);
				//Add new tasks
				createTasks(tasks);
			}
			
		}
		
		private List<String> filterTasks(List<String> newTasks) {
			TaskService taskService = new TaskService();			
			List<Task> existingTasks = taskService.GetFolder("FOG").AllTasks.ToList();
			
			foreach(Task task in existingTasks) {
				if(!newTasks.Contains(task.Name)) {
					LogHandler.Log(getName(), "Delete task " + task.Name);
					taskService.RootFolder.DeleteTask(@"FOG\" + task.Name, true); //If the existing task is not in the new list delete it
				} else {
					LogHandler.Log(getName(), "Removing " + task.Name + " from queue");
					newTasks.Remove(task.Name); //Remove the existing task from the queue
				}
			}
			
			return newTasks;
		}
		
		private void createTasks(List<String> tasks) {
			TaskService taskService = new TaskService();
			
			int index = 0;

			foreach(String task in tasks) {
				//Split up the response
				String[] taskData = task.Split('@');
				
				//Create task definition
				TaskDefinition taskDefinition = taskService.NewTask();
				taskDefinition.RegistrationInfo.Description = task;
				taskDefinition.Principal.UserId = "SYSTEM";
				
				//Create task trigger
				DailyTrigger trigger = new DailyTrigger(1);
				trigger.StartBoundary = DateTime.Today + TimeSpan.FromHours(int.Parse(taskData[0])) + TimeSpan.FromMinutes(int.Parse(taskData[1])); //Run at the specified time
				
				taskDefinition.Triggers.Add(trigger);
				
				//Create task action
				if(taskData[2].Equals("r")) {
					taskDefinition.Actions.Add(new ExecAction("shutdown.exe", "/r /c \"Green FOG\" /t 0", null));
				} else if(taskData[2].Equals("s")) {
					taskDefinition.Actions.Add(new ExecAction("shutdown.exe", "/s /c \"Green FOG\" /t 0", null));
				}
				
				//Register the task
				try {
					taskService.RootFolder.RegisterTaskDefinition(@"FOG\" + task, taskDefinition);
					LogHandler.Log(getName(), "Registered task: " + task);
				} catch (Exception ex) {
					LogHandler.Log(getName(), "Error registering task: " + task);
					LogHandler.Log(getName(), "ERROR: " + ex.Message);
				}
				
				index++;
			}
			
		}
	}
}