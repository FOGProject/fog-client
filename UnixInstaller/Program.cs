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
using System.IO;
using System.IO.Compression;
using System.Reflection;
using FOG.Core;

namespace FOG
{
    class UnixInstaller
    {
        private const string LogName = "Installer";
        private const string Location = "/opt/fog-service";

        static void Main(string[] args)
        {
            if (args.Length != 6) return;
            Log.Output = Log.Mode.Console;

            var url = args[0];
            var tray = args[1];
            var version = args[2];
            var company = args[3];
            var rootLog = args[4];
            var https = args[5];

            var baseURL = url;
            var webRoot = "";
            try
            {
                if (url.Contains("/"))
                {
                    baseURL = url.Substring(0, url.IndexOf("/"));
                    webRoot = url.Substring(url.IndexOf("/"));
                }
            }
            catch (Exception)
            {
                // ignored
            }


            ExtractFiles();
            AdjustPermissions();
            GenericSetup.SaveSettings(https, tray, baseURL, webRoot, version, company, rootLog, Location);
            GenericSetup.PinServerCert(Location);
        }

        private static void AdjustPermissions()
        {
            var logLocation = Path.Combine(Location, "fog.log");

            if (!File.Exists(logLocation))
                File.Create(logLocation);

            ProcessHandler.Run("chmod", "755 " + logLocation);
        }

        private static void ExtractFiles()
        {
            var tmpLocation = Path.Combine("/opt/", "FOGService.zip");

            ExtractResource("FOGService.zip", tmpLocation);
            ZipFile.ExtractToDirectory(tmpLocation, Location);
            File.Delete(tmpLocation);
        }

        private static void ExtractResource(string resource, string filePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var input = assembly.GetManifestResourceStream(resource))
            using (var output = File.Create(filePath))
            {
                CopyStream(input, output);
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
