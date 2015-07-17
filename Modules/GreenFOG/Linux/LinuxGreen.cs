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
using System.Reflection;
using FOG.Handlers;

namespace FOG.Modules.GreenFOG
{
    class LinuxGreen : IGreen
    {
        public void AddTask(int min, int hour, bool restart)
        {
            var filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Power.exe");
            var command = "";

            command = string.Format("crontab -l | {{ cat; echo \"{0} {1} * * mono {2} {3}\"; }} | crontab -", min, hour, filepath, 
                restart 
                ? "reboot \"This computer is going to reboot.\"" 
                : "shutdown \"This computer is going to shutdown to save power.\"");

            ProcessHandler.Run(command, "", true);
            
        }
        
        public void RemoveTask(int min, int hour, bool restart)
        {
            throw new NotImplementedException();
        }

        public List<string> FilterTasks(List<string> tasks)
        {
            throw new NotImplementedException();
        }
    }
}
