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
using Zazzles.Modules;


namespace FOG.Modules.GreenFOG
{
    /// <summary>
    ///     Perform cron style power tasks
    /// </summary>
    public sealed class GreenFOG : PolicyModule<GreenFOGMessage>
    {
        public override string Name { get; protected set; }
        public override Settings.OSType Compatiblity { get; protected set; }
        private readonly IGreen _instance;

        public GreenFOG()
        {
            Name = "GreenFOG";
            Compatiblity = Settings.OSType.All;

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

        private void CreateTasks(IEnumerable<GreenFOGTask> tasks)
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

        protected override void OnEvent(GreenFOGMessage message)
        {
            ClearAll();
            CreateTasks(message.Tasks);
        }
    }
}