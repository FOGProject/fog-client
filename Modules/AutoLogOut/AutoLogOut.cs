/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
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

using System.Threading;
using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;

namespace FOG.Modules.AutoLogOut
{
    /// <summary>
    ///     Automatically log out the user after a given duration of inactivity
    /// </summary>
    public class AutoLogOut : AbstractModule<DataContracts.AutoLogOut>
    {
        private readonly int _minimumTime;

        public AutoLogOut()
        {
            Name = "AutoLogOut";
            _minimumTime = 300;
        }

        protected override void DoWork(Response data, DataContracts.AutoLogOut msg)
        {
            if (!User.AnyLoggedIn())
            {
            	Log.Debug(Name, "No user logged in");
            	return;
            }

            var timeOut = GetTimeOut(msg);
            if (timeOut <= 0) return;

            var inactiveTime = User.InactivityTime();

            Log.Debug(Name, $"Time set to {timeOut} seconds");
            Log.Debug(Name, $"Inactive for {inactiveTime} seconds");

            if (inactiveTime < timeOut) return;

            Notification.Emit(ALOStrings.NOTIFICATION_TITLE, ALOStrings.NOTIFICATION_BODY, global: false);

            //Wait 20 seconds and check if the user is no longer inactive
            Thread.Sleep(20000);
            if (User.InactivityTime() >= timeOut)
                Power.LogOffUser();
        }

        //Get how long a user must be inactive before logging them out
        private int GetTimeOut(DataContracts.AutoLogOut msg)
        {
            return msg.Time >= _minimumTime ? msg.Time : 0;
        }
    }
}
