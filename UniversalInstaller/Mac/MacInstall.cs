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
using System.IO.Compression;
using System.Linq;
using Zazzles;

namespace FOG
{
    internal class MacInstall : IInstall
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
            Helper.ExtractResource("FOG.Scripts.fog.agent", Path.Combine(GetLocation(), "fog.agent"), true);
            Helper.ExtractResource("FOG.Scripts.fog.daemon", Path.Combine(GetLocation(), "fog.daemon"), true);
            Helper.ExtractResource("FOG.Scripts.org.freeghost.daemon.plist", "/Library/LaunchDaemons/org.freeghost.daemon.plist", true);
            Helper.ExtractResource("FOG.Scripts.org.freeghost.useragent.plist", "/Library/LaunchAgents/org.freeghost.useragent.plist", true);
            Helper.ExtractResource("FOG.Scripts.osxbind.sh", Path.Combine(GetLocation(), "osxbind.sh"), true);
            Helper.ExtractResource("FOG.Scripts.control.sh", Path.Combine(GetLocation(), "control.sh"), true);

            var trayZip = Path.Combine(GetLocation(), "OSXFOGTray.zip");
            Helper.ExtractResource("FOG.Scripts.OSXFOGTray.zip", trayZip);
            ZipFile.ExtractToDirectory(trayZip, GetLocation());
            File.Delete(trayZip);

            ProcessHandler.Run("chmod", "755 " + Path.Combine(GetLocation(), "fog.daemon"));
            ProcessHandler.Run("chmod", "755 " + Path.Combine(GetLocation(), "fog.agent"));
            ProcessHandler.Run("chmod", "755 " + Path.Combine(GetLocation(), "osxbind.sh"));
            ProcessHandler.Run("chmod", "755 " + Path.Combine(GetLocation(), "control.sh"));
            ProcessHandler.Run("chmod", "755 " + Path.Combine(GetLocation(), "OSX-FOG-TRAY.app"));
            ProcessHandler.Run("chmod", "+x " + Path.Combine(GetLocation(), "OSX-FOG-TRAY.app/Contents/MacOS/OSX-FOG-Tray"));

            // Ensure correct ownership or daemons will not launch
            ProcessHandler.Run("chown", "root /Library/LaunchDaemons/org.freeghost.daemon.plist");
            ProcessHandler.Run("chown", "root /Library/LaunchAgents/org.freeghost.useragent.plist");

            // Load daemon with permanent add to launchd using legacy syntax
            ProcessHandler.Run("launchctl", "load -w /Library/LaunchDaemons/org.freeghost.daemon.plist");
            
            return true;
        }

        public bool Install(string https, string tray, string server, string webRoot, string company, string rootLog, string location)
        {
            return Install();
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
            
        }

        public bool Uninstall()
        {
            if (Directory.Exists(GetLocation()))
            {
                // Check if an upgrade is underway
                if (Settings.Location.Contains(GetLocation()))
                {
                    var filePaths = Directory.GetFiles(GetLocation(), "*", SearchOption.TopDirectoryOnly);
                    foreach (var filePath in filePaths.Where(filePath => !filePath.ToLower().EndsWith("fog.log")))
                        File.Delete(filePath);

                    // OSX .app programs are actually directories
                    var trayApp = Path.Combine(GetLocation(), "OSX-FOG-TRAY.app");
                    if (Directory.Exists(trayApp))
                        Directory.Delete(trayApp, true);

                }
                else
                {
                    Directory.Delete(GetLocation(), true);
                }
            }

            ProcessHandler.Run("launchctl", "unload -w /Library/LaunchDaemons/org.freeghost.daemon.plist");
            ProcessHandler.Run("launchctl", "unload -w /Library/LaunchAgents/org.freeghost.useragent.plist");
            File.Delete("/Library/LaunchAgents/com.freeghost.useragent.plist");
            File.Delete("/Library/LaunchDaemons/com.freeghost.daemon.plist");
            return true;
        }
    }
}
