/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2016 FOG Project
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
using System.IO;
using Zazzles;

namespace FOG.Modules.HostnameChanger.Mac
{
    internal class MacHostName : IHostName
    {
        private readonly string Name = "HostnameChanger";

        public void RenameComputer(string hostname)
        {
            ProcessHandler.Run("scutil", "--set HostName " + hostname);
            ProcessHandler.Run("scutil", "--set LocalHostName " + hostname);
            ProcessHandler.Run("scutil", "--set ComputerName " + hostname);
        }

        public bool RegisterComputer(DataContracts.HostnameChanger msg)
        {
            var returnCode = ProcessHandler.Run("/bin/bash",
                $"{Path.Combine(Settings.Location, "osxbind.sh")} {msg.ADDom} {msg.ADUser} {msg.ADPass} {msg.ADOU}");

            return returnCode == 0;
        }

        public void UnRegisterComputer(DataContracts.HostnameChanger msg)
        {
            throw new NotImplementedException();
        }

        public void ActivateComputer(string key)
        {
            throw new NotImplementedException();
        }
    }
}