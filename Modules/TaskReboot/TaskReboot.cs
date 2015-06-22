﻿/*
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

using FOG.Handlers;
using FOG.Handlers.Middleware;
using FOG.Handlers.Power;

namespace FOG.Modules.TaskReboot
{
    /// <summary>
    ///     Reboot the computer if a task needs to
    /// </summary>
    public class TaskReboot : AbstractModule
    {
        //This variable is used to detect if the user has been told there is a pending shutdown
        private bool _notifiedUser;

        public TaskReboot()
        {
            Name = "TaskReboot";
            _notifiedUser = false;
        }

        protected override void DoWork()
        {
            //Get task info
            var response = Communication.GetResponse("/service/jobs.php", true);

            //Shutdown if a task is avaible and the user is logged out or it is forced
            if (response.Error) return;
            
            Log.Entry(Name, "Restarting computer for task");

            if (!UserHandler.IsUserLoggedIn() || response.GetField("#force").Equals("1"))
                Power.Restart(Name);

            else if (!response.Error && !_notifiedUser)
            {
                Log.Entry(Name, "User is currently logged in, will try again later");

                var notification = new Notification("Please log off",
                    string.Format(
                        "{0} is attemping to service your computer, please log off at the soonest available time",
                        RegistryHandler.GetSystemSetting("Company")), 60);

                Bus.Emit(Bus.Channel.Notification, notification.GetJson(), true);
                _notifiedUser = true;
            }
        }
    }
}