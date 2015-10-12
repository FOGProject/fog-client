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

using System.IO;
using FOG.Core;

namespace FOG
{
    internal class LinuxInstall : IInstall
    {
        public bool PrepareFiles()
        {
            return true;
        }

        public bool Install()
        {
            Helper.ExtractFiles("/opt/", GetLocation());

            var logLocation = Path.Combine(GetLocation(), "fog.log");
            if (!File.Exists(logLocation))
                File.Create(logLocation);
            ProcessHandler.Run("chmod", "755 " + logLocation);

            Helper.ExtractResource("FOG.Scripts.init-d", "/etc/init.d/FOGService");
            ProcessHandler.Run("chmod", "755 /etc/init.d/FOGService");

            if(ProcessHandler.Run("systemctl", "enable FOGService >/ dev / null 2 > &1") != 0)
                ProcessHandler.Run("sysv-rc-conf", "FOGService on >/ dev / null 2 > &1");

            return true;
        }

        public bool Configure()
        {
            return true;
        }

        public string GetLocation()
        {
            return "/opt/fog-service";
        }

        public bool Uninstall()
        {
            Directory.Delete(GetLocation());
            ProcessHandler.Run("systemctl", "disable FOGService >/ dev / null 2 > &1");
            ProcessHandler.Run("sysv-rc-conf", "FOGService off >/ dev / null 2 > &1");
            File.Delete("/etc/init.d/FOGService");
            return true;
        }
    }
}
