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
using FOG.Commands.Core.CBus;
using FOG.Commands.Core.Middleware;
using FOG.Commands.Core.User;
using FOG.Commands.Modules;
using FOG.Handlers;

namespace FOG
{
    internal class Program
    {
        private const string Name = "Console";

        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>
        {
            {"modules", new ModuleCommand()},
            {"bus", new BusCommand()},
            {"middleware", new MiddlewareCommand()},
            {"user", new UserCommand()}
        };
 
        public static void Main(string[] args)
        {
            Log.Output = Log.Mode.Console;
            Eager.Initalize();

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
                if (ProcessCommand(input.ToLower().Split(' '))) break;
                Log.Divider();
            }
            Bus.Dispose();
        }

        private static bool ProcessCommand(string[] command)
        {
            if (command.Length == 0) return false;
            if (command.Length == 1 && command[0].Equals("exit")) return true;

            if (command[0].Equals("?") || command[0].Equals("help"))
            {
                Help();
                return false;            
            }

            if (command.Length > 1 && _commands.ContainsKey(command[0]))
                if (_commands[command[0]].Process(command.Skip(1).ToArray()))
                    return false;

            Log.Error(Name, "Unknown command");

            return false;
        }

        private static void Help()
        {
            Log.WriteLine("Avaible commands (append ? to any command for more information)");
            foreach(var keyword in _commands.Keys)
            {
                Log.WriteLine("--> " + keyword);
            }
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