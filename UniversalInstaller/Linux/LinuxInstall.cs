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

using System.IO;
using System.Linq;
using Zazzles;

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
            if (Directory.Exists(GetLocation()))
                Uninstall();

            if (!Directory.Exists("/opt/"))
            {
                Directory.CreateDirectory("/opt/");
                ProcessHandler.Run("chown", "root:root /opt/");
                ProcessHandler.Run("chmod", "0755 /opt/");
            }

            Helper.ExtractFiles("/opt/", GetLocation());

            var logLocation = Path.Combine(GetLocation(), "fog.log");
            if (!File.Exists(logLocation))
                File.Create(logLocation);
            ProcessHandler.Run("chmod", "755 " + logLocation);

            Helper.ExtractResource("FOG.Scripts.control.sh", Path.Combine(GetLocation(), "control.sh"), true);
            ProcessHandler.Run("chmod", "755 " + Path.Combine(GetLocation(), "control.sh"));

            return AddControlScripts();
        }

        public bool Install(string https, string tray, string server, string webRoot, string company, string rootLog, string location)
        {
            return Install();
        }

        public bool isSystemd()
        {
            var systemd = ProcessHandler.Run("pidof", "systemd");
            return systemd == 0;
        }

        public bool isInitd()
        {
            var initd = ProcessHandler.Run("pidof", "init");
            return initd == 0;
        }

        private bool AddControlScripts()
        {
            var systemd = isSystemd();
            var initd = isInitd();

            if (!systemd && !initd) return false;

            if (systemd)
            {
                var path = "";
                var path1 = "/lib/systemd/system";
                var path2 = "/usr/lib/systemd/system";

                if (Directory.Exists(path1))
                    path = path1;
                else if (Directory.Exists(path2))
                    path = path2;
                else
                    return false;

                Helper.ExtractResource("FOG.Scripts.systemd", Path.Combine(path,"FOGService.service"), true);
                ProcessHandler.Run("chmod", "755 " + Path.Combine(path,"FOGService.service"));
                ProcessHandler.Run("systemctl", "enable FOGService.service");
            }
            else if (initd)
            {
                Helper.ExtractResource("FOG.Scripts.init-d", "/etc/init.d/FOGService", true);
                ProcessHandler.Run("chmod", "755 /etc/init.d/FOGService");
                if(ProcessHandler.Run("sysv-rc-conf", "FOGService on") != 0)
                    ProcessHandler.Run("chkconfig", "FOGService on");
            }
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

        public void PrintInfo()
        {
            UniversalInstaller.PrintInfo("Systemd", isSystemd().ToString());
            UniversalInstaller.PrintInfo("Initd", isInitd().ToString());
        }

        public bool Uninstall()
        {
            var systemd = isSystemd();
            var initd = isInitd();

            if (Directory.Exists(GetLocation()))
            {
                if (Settings.Location.Contains(GetLocation()))
                {
                    var filePaths = Directory.GetFiles(GetLocation(),"*",SearchOption.TopDirectoryOnly);
                    foreach (var filePath in filePaths.Where(filePath => !filePath.ToLower().EndsWith("fog.log")))
                    {
                        File.Delete(filePath);
                    }
                }
                else
                {
                    Directory.Delete(GetLocation(), true);
                }
            }

            if (systemd)
                ProcessHandler.Run("systemctl", "disable FOGService");
            else if (initd)
            {
                ProcessHandler.Run("sysv-rc-conf", "FOGService off");
                ProcessHandler.Run("chkconfig", "FOGService off");
            }

            if (File.Exists("/etc/init.d/FOGService"))
                File.Delete("/etc/init.d/FOGService");
            if (File.Exists("/lib/systemd/system/FOGService.service"))
                File.Delete("/lib/systemd/system/FOGService.service");
            if (File.Exists("/usr/lib/systemd/system/FOGService.service"))
                File.Delete("/usr/lib/systemd/system/FOGService.service");

            return true;
        }
    }
}
