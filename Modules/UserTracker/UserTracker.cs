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

using System.Collections.Generic;
using System.Net;
using FOG.Handlers;
using FOG.Handlers.Middleware;


namespace FOG.Modules.UserTracker
{
    /// <summary>
    ///     Report what users log on or off and at what time
    /// </summary>
    public class UserTracker : AbstractModule
    {
        private List<string> _usernames;

        public UserTracker()
        {
            Name = "UserTracker";
            _usernames = new List<string>();
        }

        protected override void DoWork()
        {
            var newUsernames = UserHandler.GetUsersLoggedIn();

            foreach (var username in newUsernames)
                // Remove users that are have remained logged in
                if (!_usernames.Contains(username))
                    Communication.Contact(string.Format("/service/usertracking.report.php?action=login&user={0}\\{1}", Dns.GetHostName(), username), true);
                else
                    _usernames.Remove(username);

            // Any users left in the usernames list have logged out
            foreach (var username in _usernames)
                Communication.Contact(string.Format("/service/usertracking.report.php?action=logout&user={0}\\{1}", Dns.GetHostName(), username), true);

            _usernames = newUsernames;
        }
    }
}