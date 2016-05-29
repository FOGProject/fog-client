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

using System.Collections.Generic;
using FOG.Modules.PowerManagement.CronNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;


namespace FOG.Modules.PowerManagement
{
    /// <summary>
    ///     Perform cron style power tasks
    /// </summary>
    public class PowerManagement : AbstractModule<PowerManagementMessage>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PowerAction
        {
            Shutdown,
            Restart,
            None
        }

        private readonly CronDaemon _cronDaemon;

        public PowerManagement()
        {
            Name = "PowerManagement";
            _cronDaemon = new CronDaemon();
            _cronDaemon.Start();
        }

        protected override void DoWork(Response data, PowerManagementMessage msg)
        {
            //Shutdown if a task is avaible and the user is logged out or it is forced
            if (data.Error)
            {
                ClearAll();
                return;
            }

            if (!data.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                ClearAll();
                return;
            }

            switch (msg.OnDemandAction)
            {
                case PowerAction.Shutdown:
                    Log.Entry(Name, "On demand shutdown requested");
                    ClearAll();
                    Power.Shutdown("FOG PowerManagement");
                    return;
                case PowerAction.Restart:
                    Log.Entry(Name, "On demand restart requested");
                    ClearAll();
                    Power.Restart("FOG PowerManagement");
                    return;
            }

            CreateTasks(msg.Tasks);
        }

        private void ClearAll()
        {
            _cronDaemon.RemoveAllJobs();
        }

        private void CreateTasks(List<Task> tasks)
        {
            ClearAll();
            foreach (var task in tasks)
            {
                switch (task.Action)
                {
                    case PowerAction.Shutdown:
                        _cronDaemon.AddJob(task.CRONTask, () => {Power.Shutdown("FOG PowerManagement");});
                        break;
                    case PowerAction.Restart:
                        _cronDaemon.AddJob(task.CRONTask, () => { Power.Restart("FOG PowerManagement"); });
                        break;
                }

            }
        }
    }
}