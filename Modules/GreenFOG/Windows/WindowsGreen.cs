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
using System.IO;
using System.Linq;
using System.Reflection;
using FOG.Handlers;
using Microsoft.Win32.TaskScheduler;

namespace FOG.Modules.GreenFOG
{
    class WindowsGreen : IGreen
    {
         string LogName = "GreenFOG";

        public void AddTask(int min, int hour, bool restart)
        {
            var taskService = new TaskService();

            //Create task definition
            var taskDefinition = taskService.NewTask();
            taskDefinition.RegistrationInfo.Description = min+"@"+hour+"@"+((restart) ? "r" : "s");
            taskDefinition.Principal.UserId = "SYSTEM";

            var trigger = new DailyTrigger()
            {
                StartBoundary = DateTime.Today + TimeSpan.FromHours(hour) + TimeSpan.FromMinutes(min)
            };

            taskDefinition.Triggers.Add(trigger);

            //Create task action
            var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +@"Power.exe");
            
            taskDefinition.Actions.Add(restart
                ? new ExecAction(fileName, "reboot \"This computer is going to reboot.\"")
                : new ExecAction(fileName, "shutdown \"This computer is going to shutdown to save power.\""));

            taskDefinition.Settings.AllowDemandStart = false;
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.DisallowStartOnRemoteAppSession = false;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;

            taskService.RootFolder.RegisterTaskDefinition(@"FOG\" + taskDefinition.RegistrationInfo.Description, taskDefinition);
        }

        public void RemoveTask(int min, int hour, bool restart)
        {
            throw new NotImplementedException();
        }

        public List<string> FilterTasks(List<string> tasks)
        {
            var taskService = new TaskService();
            
            try
            {
                taskService.RootFolder.CreateFolder("FOG");
            }
            catch (Exception)
            {
                // ignored
            }

            var existingTasks = taskService.GetFolder("FOG").AllTasks.ToList();

            foreach (var task in existingTasks)
                if (!tasks.Contains(task.Name))
                {
                    Log.Entry(LogName, "Delete task " + task.Name);
                    taskService.RootFolder.DeleteTask(@"FOG\" + task.Name);
                    //If the existing task is not in the new list delete it
                }
                else
                {
                    Log.Entry(LogName, task.Name + " already scheduled");
                    tasks.Remove(task.Name); //Remove the existing task from the queue
                }

            return tasks;
        }
    }
}
