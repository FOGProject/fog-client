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

using System;
using System.IO;
using Zazzles;
using Zazzles.Debugger.Commands;
using Zazzles.Middleware;

namespace FOG.Commands
{
    internal class PowerCommand : ICommand
    {
        private const string LogName = "Console::Power";

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }
            else if (args[0].Equals("abort", StringComparison.OrdinalIgnoreCase))
            {
                Power.AbortShutdown();
            }
            else if (args[0].Equals("shutdown", StringComparison.OrdinalIgnoreCase))
            {
                Power.Shutdown("fog client debugger shutdown");
            }
            else if (args[0].Equals("restart", StringComparison.OrdinalIgnoreCase))
            {
                Power.Restart("fog client debugger restart");
            }
            else if (args[0].Equals("logoff", StringComparison.OrdinalIgnoreCase))
            {
                Power.LogOffUser();
            }
            else if (args[0].Equals("hibernate", StringComparison.OrdinalIgnoreCase))
            {
                Power.Hibernate();
            }
            else if (args[0].Equals("lock", StringComparison.OrdinalIgnoreCase))
            {
                Power.LockWorkStation();
            }
            return true;
        }

        private void Help()
        {
            Log.WriteLine("Available commands");
            Log.WriteLine("--> abort");
            Log.WriteLine("--> shutdown");
            Log.WriteLine("--> restart");
            Log.WriteLine("--> logoff");
            Log.WriteLine("--> hibernate");
            Log.WriteLine("--> lock");

        }
    }
}