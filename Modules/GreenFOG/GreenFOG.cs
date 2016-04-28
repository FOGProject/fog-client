/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2016 FOG Project
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
using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;


namespace FOG.Modules.GreenFOG
{
    /// <summary>
    ///     Perform cron style power tasks
    /// </summary>
    public class GreenFOG : AbstractModule<GreenFOGMessage>
    {
        private readonly IGreen _instance;

        public GreenFOG()
        {
            Compatiblity = Settings.OSType.Windows;
            Name = "GreenFOG";

            switch (Settings.OS)
            {
                case Settings.OSType.Windows:
                    _instance = new WindowsGreen();
                    break;
                default:
                    _instance = new UnixGreen();
                    break;
            }
        }

        protected override void DoWork(Response data, GreenFOGMessage msg)
        {
            //Shutdown if a task is avaible and the user is logged out or it is forced
            if (data.Error) return;

            if (!data.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            ClearAll();

            //Add new tasks
            CreateTasks(msg.Tasks);
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

        private void CreateTasks(IEnumerable<Task> tasks)
        {
            foreach (var task in tasks)
            {
                try
                {
                    _instance.AddTask(task.Min, task.Hour, task.Action.Equals("reboot"));
                    Log.Entry(Name, "Registered task: " + task.Action + " at " + task.Hour + ":" + task.Min);
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