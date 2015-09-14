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
using System.Linq;
using FOG.Handlers;

namespace FOG.Commands.Core.Middleware
{
    internal class MiddlewareCommand : ICommand
    {
        private const string LogName = "Console::Middleware";

        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>
        {
            {"authentication", new AuthenticationCommand()},
            {"configuration", new ConfigurationCommand()},
            {"communication", new CommunicationCommand()}
        };

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }

            return _commands.Count > 1 && _commands.ContainsKey(args[0]) &&
                   _commands[args[0]].Process(args.Skip(1).ToArray());
        }

        private static void Help()
        {
            Log.WriteLine("Available commands (append ? to any command for more information)");
            foreach (var keyword in _commands.Keys)
            {
                Log.WriteLine("--> " + keyword);
            }
        }
    }
}