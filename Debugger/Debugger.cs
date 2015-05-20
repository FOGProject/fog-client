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
        private const string Server = "https://fog.jbob.io/fog";
        private const string MAC = "1a:2b:3c:4d:5e:6f";
        private const string Name = "Console";
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

            LogHandler.PaddedHeader("FOG Console");
            CommunicationHandler.GetAndSetServerAddress();
            LogHandler.Log(Name, "Type help for a list of commands");
            LogHandler.NewLine();
            InteractiveShell();
        }

        private static void InteractiveShell()
        {
            while (true)
            {
                LogHandler.Write("fog: ");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input)) continue;
                if (ProcessCommand(input.ToLower().Split(' '))) break;
                LogHandler.Divider();
            }
        }

        private static bool ProcessCommand(string[] command)
        {
            if (command.Length == 0) return false;
            if (command.Length == 1 && command[0].Equals("exit")) return true;

            // Check modules
            if (_modules.ContainsKey(command[0]))
            {
                _modules[command[0]].Start();
                LogHandler.NewLine();
            }

            // Check custom commands
            else if (command[0].Equals("authenticate"))
                CommunicationHandler.Authenticate();
            else if (command[0].Equals("info"))
            {
                LogHandler.Log(Name, "Server: " + CommunicationHandler.ServerAddress);
                LogHandler.Log(Name, "MAC: " + CommunicationHandler.GetMacAddresses());
            }

            else if (command.Length == 3 && command[0].Equals("configure"))
            {
                if (command[1].Equals("server"))
                    CommunicationHandler.ServerAddress = command[2];
                else if (command[1].Equals("mac"))
                    CommunicationHandler.TestMAC = command[2];
            }

            else if (command.Length == 2 && command[0].Equals("configure") && command[1].Equals("default"))
            {
                CommunicationHandler.ServerAddress = Server;
                CommunicationHandler.TestMAC = MAC;
            }

            else if (command.Length == 1 && command[0].Equals("help"))
            {
                LogHandler.WriteLine(" authenticate <-- Authenticates the debugger shell");
                LogHandler.WriteLine(" configue server ____ <-- Sets the server address");
                LogHandler.WriteLine(" configue mac ____ <-- Sets the mac address");
                LogHandler.WriteLine(" configue default <-- Sets the default testing mac and server address");
                foreach (var module in _modules.Keys)
                {
                    LogHandler.WriteLine(" " + module + "<-- Runs this specific module");
                }
                LogHandler.WriteLine(" exit <-- Exits the console");
            }

            else
            {
                LogHandler.Log(Name, "Unknown command");
            }

            return false;
        }

    }
}