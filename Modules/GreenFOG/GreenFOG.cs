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
using System.Linq;
using FOG.Handlers;
using FOG.Handlers.Middleware;
using Microsoft.Win32.TaskScheduler;

namespace FOG.Modules.GreenFOG
{
    /// <summary>
    ///     Perform cron style power tasks
    /// </summary>
    public class GreenFOG : AbstractModule
    {
        public GreenFOG()
        {
            Name = "GreenFOG";
        }

        protected override void DoWork()
        {
            //Get actions
            var response = Communication.GetResponse("/service/greenfog.php", true);

            //Shutdown if a task is avaible and the user is logged out or it is forced
            if (response.Error) return;

            var tasks = response.GetList("#task", false);

            //Filter existing tasks
            tasks = FilterTasks(tasks);
            //Add new tasks
            CreateTasks(tasks);
        }

        private List<string> FilterTasks(List<string> newTasks)
        {
            var taskService = new TaskService();
            var existingTasks = taskService.GetFolder("FOG").AllTasks.ToList();

            foreach (var task in existingTasks)
                if (!newTasks.Contains(task.Name))
                {
                    Log.Entry(Name, "Delete task " + task.Name);
                    taskService.RootFolder.DeleteTask(@"FOG\" + task.Name);
                    //If the existing task is not in the new list delete it
                }
                else
                {
                    Log.Entry(Name, "Removing " + task.Name + " from queue");
                    newTasks.Remove(task.Name); //Remove the existing task from the queue
                }

            return newTasks;
        }

        private void CreateTasks(IEnumerable<string> tasks)
        {
            var taskService = new TaskService();

            foreach (var task in tasks)
            {
                var taskData = task.Split('@');

                //Create task definition
                var taskDefinition = taskService.NewTask();
                taskDefinition.RegistrationInfo.Description = task;
                taskDefinition.Principal.UserId = "SYSTEM";

                var trigger = new DailyTrigger()
                {
                    StartBoundary = DateTime.Today + TimeSpan.FromHours(int.Parse(taskData[0])) +
                                    TimeSpan.FromMinutes(int.Parse(taskData[1]))
                };

                taskDefinition.Triggers.Add(trigger);

                //Create task action
                if (taskData[2].Equals("r"))
                    taskDefinition.Actions.Add(new ExecAction("shutdown.exe", "/r /c \"Green FOG\" /t 0"));
                else if (taskData[2].Equals("s"))
                    taskDefinition.Actions.Add(new ExecAction("shutdown.exe", "/s /c \"Green FOG\" /t 0"));

                //Register the task
                try
                {
                    taskService.RootFolder.RegisterTaskDefinition(@"FOG\" + task, taskDefinition);
                    Log.Entry(Name, "Registered task: " + task);
                }
                catch (Exception ex)
                {
                    Log.Error(Name, "Could not register task: " + task);
                    Log.Error(Name, ex);
                }
            }
        }
    }
}