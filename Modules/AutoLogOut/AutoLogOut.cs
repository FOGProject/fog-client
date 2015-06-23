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

using System;
using System.Threading;
using FOG.Handlers;
using FOG.Handlers.Middleware;
using FOG.Handlers.Power;


namespace FOG.Modules.AutoLogOut
{
    /// <summary>
    ///     Automatically log out the user after a given duration of inactivity
    /// </summary>
    public class AutoLogOut : AbstractModule
    {
        private readonly int _minimumTime;

        public AutoLogOut()
        {
            Name = "AutoLogOut";
            _minimumTime = 300;
        }

        protected override void DoWork()
        {
            if (UserHandler.IsUserLoggedIn())
            {
                //Get task info
                var taskResponse = Communication.GetResponse("/service/autologout.php", true);

                if (taskResponse.Error) return;
                var timeOut = GetTimeOut(taskResponse);
                if (timeOut <= 0) return;

                Log.Entry(Name, string.Format("Time set to {0} seconds", timeOut));
                Log.Entry(Name, string.Format("Inactive for {0} seconds", UserHandler.GetInactivityTime()));
                
                if (UserHandler.GetInactivityTime() < timeOut) return;

                var notification = new Notification("You are about to be logged off",
                    "Due to inactivity you will be logged off if you remain inactive", 20);

                Bus.Emit(Bus.Channel.Notification, notification.GetJson(), true);

                //Wait 20 seconds and check if the user is no longer inactive
                Thread.Sleep(20000);
                if (UserHandler.GetInactivityTime() >= timeOut)
                    Power.LogOffUser();
            }
            else
            {
                Log.Entry(Name, "No user logged in");
            }
        }

        //Get how long a user must be inactive before logging them out
        private int GetTimeOut(Response taskResponse)
        {
            try
            {
                var timeOut = int.Parse(taskResponse.GetField("#time"));
                if (timeOut >= _minimumTime)
                    return timeOut;

                Log.Entry(Name, "Time set is less than 1 minute");
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Unable to parse time set");
                Log.Error(Name, ex);
            }

            return 0;
        }
    }
}