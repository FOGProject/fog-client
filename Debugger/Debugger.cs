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
using System.Collections.Generic;
using System.Linq;
using FOG.Commands;
using Zazzles;

namespace FOG
{
    internal class Debugger
    {
        private const string Name = "Console";
        private static Zazzles.Debugger _instance;

        public static void Main(string[] args)
        {
            Log.Output = Log.Mode.Console;
            Eager.Initalize();

            _instance = new Zazzles.Debugger();
            _instance.AddCommand("module", new ModuleCommand());

            Log.PaddedHeader("FOG Console");
            Log.Entry(Name, "Type ? for a list of commands");
            Log.NewLine();

            try
            {
                Bus.Subscribe(Bus.Channel.Debug, OnMessage);
                InteractiveShell();
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
                Console.ReadLine();
            }
        }

        private static void InteractiveShell()
        {
            while (true)
            {
                Log.Write("fog: ");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input)) continue;
                if (_instance.ProcessCommand(input.Split(' '))) break;
                Log.Divider();
            }
            Bus.Dispose();
        }

        private static void OnMessage(dynamic data)
        {
            if (data.content == null) return;

            Log.NewLine();
            Log.WriteLine("Message recieved: " + data.content.ToString());
            Log.Write("fog: ");
        }
    }
}