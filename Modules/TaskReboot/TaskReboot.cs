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


using Zazzles;
using Zazzles.Modules;

namespace FOG.Modules.TaskReboot
{
    /// <summary>
    ///     Reboot the computer if a task needs to
    /// </summary>
    public sealed class TaskReboot : AbstractModule<string>
    {
        public override string Name { get; protected set; }
        public override Settings.OSType Compatiblity { get; protected set; }
        public override EventProcessorType Type { get; protected set; }

        public TaskReboot()
        {
            Name = "TaskReboot";
            Compatiblity = Settings.OSType.All;
            Type = EventProcessorType.Synchronous;
        }

        //TODO: deprecate ShouldAbort
        public bool ShouldAbort()
        {
            //var response = Communication.GetResponse("/service/jobs.php", true);
            //return (response.Error);
            return false;
        }

        protected override void OnEvent(string message)
        {
            Log.Entry(Name, "Restarting computer. " + message );
            Power.Restart(Name, ShouldAbort, Power.ShutdownOptions.Delay);
        }
    }
}