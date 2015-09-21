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

using FOG.Core;

namespace FOG.Commands.Core.User
{
    internal class UserCommand : ICommand
    {
        private const string LogName = "Console::UserHandler";

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }

            if (args[0].Equals("loggedin"))
            {
                Log.WriteLine("--> " + UserHandler.IsUserLoggedIn());
                return true;
            }

            if (args[0].Equals("current"))
            {
                Log.WriteLine("--> " + "\"" + UserHandler.GetCurrentUser() + "\"");
                return true;
            }

            if (args[0].Equals("inactivity"))
            {
                Log.WriteLine("--> " + UserHandler.GetInactivityTime() + " seconds");
                return true;
            }

            if (args[0].Equals("list"))
            {
                var users = UserHandler.GetUsersLoggedIn();
                Log.WriteLine("--> " + "Current users logged in:");

                foreach (var user in users)
                    Log.WriteLine("----> " + user);

                return true;
            }

            return false;
        }

        private static void Help()
        {
            Log.WriteLine("Available commands");
            Log.WriteLine("--> loggedin");
            Log.WriteLine("--> current");
            Log.WriteLine("--> inactivity");
            Log.WriteLine("--> list");
        }
    }
}