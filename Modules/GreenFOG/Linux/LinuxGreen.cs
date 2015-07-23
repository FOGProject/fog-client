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
using System.IO;
using System.Linq;
using System.Reflection;
using FOG.Handlers;

namespace FOG.Modules.GreenFOG
{
    class LinuxGreen : IGreen
    {
        private const string Cronpath = @"/etc/cron.d/fog";
        private const string LogName = "GreenFOG";

        public void AddTask(int min, int hour, bool restart)
        {
            if (!File.Exists(Cronpath)) CreateCron();

            var lines = File.ReadLines(Cronpath).ToList();

            lines.Add(GenerateCommand(min, hour, restart));
            
            File.WriteAllLines(Cronpath, lines);
        }

        private void CreateCron()
        {
            var lines = new List<string>
            {
                "SHELL=/bin/bash",
                "PATH=/usr/local/sbin:/usr/local/bin:/sbin:/bin:/usr/sbin:/usr/bin"
            };
            File.WriteAllLines(Cronpath, lines);
        }

        private string GenerateCommand(int min, int hour, bool restart)
        {
            var filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Power.exe");
            var command = "";

            command = string.Format("{0} {1} * * * root mono {2} {3}", min, hour, filepath,
                restart
                ? "reboot \"This computer is going to reboot.\""
                : "shutdown \"This computer is going to shutdown to save power.\"");

            return command;
        }

        public void Reload()
        {
            ProcessHandler.WaitDispose(
                ProcessHandler.Run("/etc/init.d/cron", "reload"));
        }

        public void ClearAll()
        {
            CreateCron();
        }

        public void RemoveTask(int min, int hour, bool restart)
        {
            if (!File.Exists(Cronpath)) CreateCron();

            var lines = File.ReadLines(Cronpath).ToList();

            lines.Remove(GenerateCommand(min, hour, restart));

            File.WriteAllLines(Cronpath, lines);
        }
    }
}
