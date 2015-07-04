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

using System;
using FOG.Handlers;
using Newtonsoft.Json.Linq;

namespace FOG
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Output = Log.Mode.Console;

            if (args.Length < 2) return;

            dynamic json = new JObject();
            json.action = "help";
            json.bounce = false;

            if (args[0].Equals("shutdown"))
                json.type = "shutdown";
            else if(args[0].Equals("reboot"))
                json.type = "reboot";

            if (args.Length > 1)
                json.reason = args[1];

            Bus.Emit(Bus.Channel.Power, json, true);
            Bus.Dispose();
            Environment.Exit(0);
        }
    }
}
