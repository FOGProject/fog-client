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

namespace FOG.Modules.GreenFOG
{
    /// <summary>
    ///     Perform cron style power tasks
    /// </summary>
    public class GreenFOG : AbstractModule
    {
        private IGreen _instance;

        public GreenFOG()
        {
            Name = "GreenFOG";
            Compatiblity = Settings.OSType.Windows;

            switch (Settings.OS)
            {
                case Settings.OSType.Mac:
                    _instance = new MacGreen();
                    break;
                case Settings.OSType.Linux:
                    _instance = new LinuxGreen();
                    break;
                default:
                    _instance = new WindowsGreen();
                    break;
            }
        }

        protected override void DoWork()
        {
            //Get actions
            var response = Communication.GetResponse("/service/greenfog.php", true);

            //Shutdown if a task is avaible and the user is logged out or it is forced
            if (response.Error) return;

            var rawTasks = response.GetList("#task", false);

            ClearAll();
            //Add new tasks
            var tasks = CastTasks(rawTasks);
            CreateTasks(tasks);
        }

        private List<Task> CastTasks(List<string> rawTasks)
        {
            return (from task in rawTasks 
                    select task.Split('@') 
                    into taskData 
                    where taskData.Length == 3 
                    select new Task(int.Parse(taskData[2]), int.Parse(taskData[1]), taskData[2].Equals("r"))).ToList();
        }

        private void ClearAll()
        {
            try
            {
                _instance.ClearAll();
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not clear all tasks");
                Log.Error(Name, ex);
            }
        }

        new public bool IsEnabled()
        {
            var moduleActiveResponse = Communication.GetResponse(string.Format("{0}?moduleid={1}",
                EnabledURL, Name.ToLower()), true);

            return !moduleActiveResponse.Error;
        }

        private void CreateTasks(IEnumerable<Task> tasks)
        {
            foreach (var task in tasks)
            {
                try
                {
                    _instance.AddTask(task.Minutes, task.Hours, task.Reboot);
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