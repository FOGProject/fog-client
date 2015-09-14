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
using Newtonsoft.Json.Linq;

namespace FOG.Commands.Core.CBus
{
    internal class BusCommand : ICommand
    {
        private const string LogName = "Console::Bus";

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }

            if (args.Length == 2 && args[0].Equals("mode"))
            {
                if (args[1].Equals("server"))
                {
                    Bus.SetMode(Bus.Mode.Server);
                    return true;
                }
                if (args[1].Equals("client"))
                {
                    Bus.SetMode(Bus.Mode.Client);
                    return true;
                }
            }
            else if (args.Length == 2 && args[0].Equals("public"))
            {
                dynamic json = new JObject();
                json.content = args[1];
                Bus.Emit(Bus.Channel.Debug, json, true);
                return true;
            }
            else if (args.Length == 2 && args[0].Equals("private"))
            {
                dynamic json = new JObject();
                json.content = args[1];
                Bus.Emit(Bus.Channel.Debug, json, false);
                return true;
            }

            return false;
        }

        private static void Help()
        {
            Log.WriteLine("Available commands");
            Log.WriteLine("--> mode    [server/client]");
            Log.WriteLine("--> public  [message]");
            Log.WriteLine("--> private [message]");
        }
    }
}