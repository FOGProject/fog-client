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
using FOG.Handlers;
using FOG.Modules;
using FOG.Modules.AutoLogOut;
using FOG.Modules.ClientUpdater;
using FOG.Modules.DisplayManager;
using FOG.Modules.GreenFOG;
using FOG.Modules.HostnameChanger;
using FOG.Modules.PrinterManager;
using FOG.Modules.SnapinClient;
using FOG.Modules.TaskReboot;
using FOG.Modules.UserTracker;

namespace FOG.Commands.Modules
{
    class ModuleCommand : ICommand
    {
        private readonly Dictionary<string, AbstractModule> _modules = new Dictionary<string, AbstractModule>()
        {
            {"autologout", new AutoLogOut()},
            {"clientupdater", new ClientUpdater()},
            {"displaymanager", new DisplayManager()},
            {"greenfog", new GreenFOG()},
            {"hostnamechanger", new HostnameChanger()},
            {"printermanager", new PrinterManager()},
            {"snapinclient", new SnapinClient()},
            {"taskreboot", new TaskReboot()},
            {"usertracker", new UserTracker()}
        };

        private const string LogName = "Console::Modules";


        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }

            if (_modules.ContainsKey(args[0].ToLower()))
            {
                _modules[args[0]].Start();
                return true;
            }

            if (!_modules.ContainsKey(args[0].ToLower())) return false;
               
            _modules[args[0]].Start();
            return true;
        }

        private void Help()
        {
            Log.WriteLine("Avaible modules");
            foreach (var moduleName in _modules.Keys)
            {
                Log.WriteLine("--> " + moduleName);
            }
        }
    }
}
