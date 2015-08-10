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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FOG.Handlers;

namespace FOG
{
    public static class UserServiceSpawner
    {
        private static readonly Dictionary<string, Process> Processes = new Dictionary<string, Process>();
        private static readonly Thread UserThread;
        private static volatile bool _running;

        static UserServiceSpawner()
        {
            UserThread = new Thread(UserChecker)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "FOGUserServiceSpawner"
            };
        }

        private static void UserChecker()
        {
            while (true)
            {
                var users = UserHandler.GetUsersLoggedIn();

                foreach (var user in users)
                {
                    if (Processes.ContainsKey(user)) continue;

                    var proc = ProcessHandler.CreateImpersonatedClientEXE("FOGUserService.exe", "", user);
                    proc.Start();
                    Processes.Add(user, proc);
                }

                var loggedOff = users.Except(Processes.Keys);
                foreach (var user in loggedOff)
                {
                    Processes[user].Kill();
                    Processes[user].Dispose();
                    Processes.Remove(user);
                }

                Thread.Sleep(5*1000);
            }
        }

        public static void Start()
        {
            _running = true;
            UserThread.Start();
        }

        public static void Stop()
        {
            _running = false;
        }

        public static void KillAll()
        {
            foreach (var proc in Processes.Values)
            {
                proc.Kill();
                proc.Dispose();
            }

            Processes.Clear();
        }
    }
}