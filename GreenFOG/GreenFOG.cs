using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace FOG
{
    /// <summary>
    /// Perform cron style power tasks
    /// </summary>
    public class GreenFOG : AbstractModule
    {
		
        public GreenFOG()
        {
            Name = "GreenFOG";
            Description = "Perform cron style power tasks";
        }
		
        protected override void doWork()
        {
            //Get actions
            var tasksResponse = CommunicationHandler.GetResponse("/service/greenfog.php", true);

            //Shutdown if a task is avaible and the user is logged out or it is forced
            if (!tasksResponse.Error)
            {
                var tasks = CommunicationHandler.ParseDataArray(tasksResponse, "#task", false);
				
                //Filter existing tasks
                tasks = filterTasks(tasks);
                //Add new tasks
                createTasks(tasks);
            }
			
        }
		
        private List<String> filterTasks(List<String> newTasks)
        {
            var taskService = new TaskService();			
            var existingTasks = taskService.GetFolder("FOG").AllTasks.ToList();
			
            foreach (var task in existingTasks)
            {
                if (!newTasks.Contains(task.Name))
                {
                    LogHandler.Log(Name, "Delete task " + task.Name);
                    taskService.RootFolder.DeleteTask(@"FOG\" + task.Name, true); //If the existing task is not in the new list delete it
                }
                else
                {
                    LogHandler.Log(Name, "Removing " + task.Name + " from queue");
                    newTasks.Remove(task.Name); //Remove the existing task from the queue
                }
            }
			
            return newTasks;
        }
		
        private void createTasks(List<String> tasks)
        {
            var taskService = new TaskService();
            var index = 0;

            foreach (String task in tasks)
            {
                //Split up the response
                var taskData = task.Split('@');
				
                //Create task definition
                var taskDefinition = taskService.NewTask();
                taskDefinition.RegistrationInfo.Description = task;
                taskDefinition.Principal.UserId = "SYSTEM";
				
                //Create task trigger
                var trigger = new DailyTrigger(1);
                trigger.StartBoundary = DateTime.Today + TimeSpan.FromHours(int.Parse(taskData[0])) + TimeSpan.FromMinutes(int.Parse(taskData[1])); //Run at the specified time
				
                taskDefinition.Triggers.Add(trigger);
				
                //Create task action
                if (taskData[2].Equals("r"))
                {
                    taskDefinition.Actions.Add(new ExecAction("shutdown.exe", "/r /c \"Green FOG\" /t 0", null));
                }
                else if (taskData[2].Equals("s"))
                {
                    taskDefinition.Actions.Add(new ExecAction("shutdown.exe", "/s /c \"Green FOG\" /t 0", null));
                }
				
                //Register the task
                try
                {
                    taskService.RootFolder.RegisterTaskDefinition(@"FOG\" + task, taskDefinition);
                    LogHandler.Log(Name, "Registered task: " + task);
                }
                catch (Exception ex)
                {
                    LogHandler.Log(Name, "Error registering task: " + task);
                    LogHandler.Log(Name, "ERROR: " + ex.Message);
                }
				
                index++;
            }
			
        }
    }
}