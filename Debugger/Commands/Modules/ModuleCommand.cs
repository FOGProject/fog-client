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
using FOG.Handlers;
using FOG.Modules;
using FOG.Modules.AutoLogOut;

namespace FOG.Commands.Modules
{
    class ModuleCommand : ICommand
    {
        private readonly Dictionary<string, AbstractModule> _modules = new Dictionary<string, AbstractModule>();
        private const string LogName = "Console::Modules";

        private void AddModule(string module)
        {
            try
            {
                switch (module.ToLower())
                {
                    case "autologout":
                        _modules.Add(module, new AutoLogOut());
                        break;
                    default:
                        Log.Error(LogName, "Unknown module name");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to add " + module);
                Log.Error(LogName, ex);
            }
        }


        public bool Process(string[] args)
        {
            if (_modules.ContainsKey(args[0].ToLower()))
            {
                _modules[args[0]].Start();
                return true;
            }

            AddModule(args[0]);
            if (!_modules.ContainsKey(args[0].ToLower())) return false;
               
            _modules[args[0]].Start();
            return true;
        }
    }
}
