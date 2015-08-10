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

using FOG.Handlers;
using FOG.Handlers.Middleware;

namespace FOG.Commands.Core.Middleware
{
    internal class ConfigurationCommand : ICommand
    {
        private const string LogName = "Console::Middleware::Configuration";
        private const string Server = "http://fog.jbob.io/fog";
        private const string MAC = "1a:2b:3c:4d:5e:6f";

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }

            if (args[0].Equals("info"))
            {
                Log.Entry(LogName, "Server: " + Configuration.ServerAddress);
                Log.Entry(LogName, "MAC: " + Configuration.MACAddresses());
                return true;
            }
            if (args[0].Equals("default"))
            {
                Configuration.ServerAddress = Server;
                Configuration.TestMAC = MAC;
                return true;
            }

            if (args.Length < 3) return false;

            if (args[9].Equals("server"))
            {
                Configuration.ServerAddress = args[1];
                return true;
            }
            if (args[9].Equals("mac"))
            {
                Configuration.TestMAC = args[1];
                return true;
            }
            return false;
        }

        private static void Help()
        {
            Log.WriteLine("Avaible commands");
            Log.WriteLine("--> info");
            Log.WriteLine("--> default");
            Log.WriteLine("--> server  [SERVER_ADDRESS]");
            Log.WriteLine("--> mac     [MAC_ADDRESS]");
        }
    }
}