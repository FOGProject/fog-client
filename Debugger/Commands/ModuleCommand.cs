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

using System.Collections.Generic;
using FOG.Modules.AutoLogOut;
using FOG.Modules.HostnameChanger;
using FOG.Modules.PowerManagement;
using FOG.Modules.PrinterManager;
using FOG.Modules.SnapinClient;
using FOG.Modules.TaskReboot;
using FOG.Modules.UserTracker;
using Zazzles;
using Zazzles.Debugger.Commands;
using Zazzles.Modules;

namespace FOG.Commands
{
    internal class ModuleCommand : ICommand
    {
        private const string LogName = "Console::Modules";

        private readonly Dictionary<string, IModule> _modules = new Dictionary<string, IModule>
        {
            {"autologout", new AutoLogOut()},
            {"powermanagement", new PowerManagement()},
            {"hostnamechanger", new HostnameChanger()},
            {"printermanager", new PrinterManager()},
            {"snapinclient", new SnapinClient()},
            {"taskreboot", new TaskReboot()},
            {"usertracker", new UserTracker()}
        };

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }

            if (_modules.ContainsKey(args[0].ToLower()))
            {
                //_modules[args[0]].Start();
                return true;
            }

            if (!_modules.ContainsKey(args[0].ToLower())) return false;

            //_modules[args[0]].Start();
            return true;
        }

        private void Help()
        {
            Log.WriteLine("Available modules");
            foreach (var moduleName in _modules.Keys)
            {
                Log.WriteLine("--> " + moduleName);
            }
        }
    }
}