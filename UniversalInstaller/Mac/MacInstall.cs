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
            Helper.ExtractFiles("/opt/", GetLocation());

            var logLocation = Path.Combine(GetLocation(), "fog.log");
            if (!File.Exists(logLocation))
                File.Create(logLocation);
            ProcessHandler.Run("chmod", "755 " + logLocation);
            Helper.ExtractResource("FOG.Scripts.fog.useragent", Path.Combine(GetLocation(), "fog.useragent"));
            ProcessHandler.Run("chmod", "755 "+ Path.Combine(GetLocation(), "fog.useragent"));
            Helper.ExtractResource("FOG.Scripts.fog.daemon", Path.Combine(GetLocation(), "fog.daemon"));
            ProcessHandler.Run("chmod", "755 "+ Path.Combine(GetLocation(), "fog.daemon"));
            Helper.ExtractResource("FOG.Scripts.com.freeghost.daemon.plist", "/Library/LaunchDaemons/com.freeghost.daemon.plist");
            ProcessHandler.Run("chown", "root /Library/LaunchDaemons/com.freeghost.daemon.plist");
            Helper.ExtractResource("FOG.Scripts.com.freeghost.useragent.plist", "/Library/LaunchAgents/com.freeghost.useragent.plist");
            ProcessHandler.Run("chown", "root /Library/LaunchAgents/com.freeghost.useragent.plist");
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
            ProcessHandler.Run("launchctl", "unload -w /Library/LaunchDaemons/org.freeghost.daemon.plist");
            ProcessHandler.Run("launchctl", "unload -w /Library/LaunchAgents/org.freeghost.useragent.plist");
            File.Delete("/Library/LaunchAgents/com.freeghost.useragent.plist");
            File.Delete("/Library/LaunchDaemons/com.freeghost.daemon.plist");
            return true;
        }
    }
}
