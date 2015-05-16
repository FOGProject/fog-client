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
using FOG.Modules.DisplayManager;
using FOG.Modules.GreenFOG;
using FOG.Modules.HostnameChanger;
using FOG.Modules.PrinterManager;
using FOG.Modules.SnapinClient;
using FOG.Modules.TaskReboot;
using FOG.Modules.UserTracker;

namespace FOG
{
    internal class Program
    {
        private const string Server = "http://209.114.111.13/fog";
        private const string MAC = "78:45:c4:be:42:8f";
        private static readonly Dictionary<string, AbstractModule> _modules = new Dictionary<string, AbstractModule>
        {
            {"autologout", new AutoLogOut()},
            {"displaymanager", new DisplayManager()},
            {"greenfog", new GreenFOG()},
            {"hostnamechanger", new HostnameChanger()},
            {"printermanager", new PrinterManager()},
            {"snapinclient", new SnapinClient()},
            {"taskreboot", new TaskReboot()},
            {"usertracker", new UserTracker()}
        };
 
        public static void Main(string[] args)
        {
            LogHandler.Mode = LogHandler.LogMode.Console;
            LogHandler.Verbose = true;

            LogHandler.PaddedHeader("Authentication");
            CommunicationHandler.ServerAddress = Server;
            CommunicationHandler.TestMAC = MAC;

            CommunicationHandler.Authenticate();

            LogHandler.PaddedHeader("Exploit");
            _modules["hostnamechanger"].Start();

            LogHandler.Write("Exiting shell.. press Enter");
            Console.ReadLine();
            //InteractiveShell();
        }

        private static void InteractiveShell()
        {
            LogHandler.Header("Interactive Debugger Shell");

            while (true)
            {
                LogHandler.Write("fog: ");
                Console.ReadLine();
                break;
            }

            LogHandler.Write("Exiting shell.. press Enter");
            Console.ReadLine();
        }

    }
}