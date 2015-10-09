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
        private const string Version = "0.10.0";

        static void Main(string[] args)
        {
            Log.Output = Log.Mode.Console;

            if (args.Length == 5)
                ProcessArgs(args);
            else if (Settings.OS == Settings.OSType.Linux)
                InteractiveMode();
            else
                return;

            ExtractFiles();
            AdjustPermissions();
            GenericSetup.PinServerCert(Location);
        }

        private static void ProcessArgs(string[] args)
        {
            var url = args[0];
            var tray = args[1];
            var company = args[2];
            var rootLog = args[3];
            var https = args[4];

            var baseURL = url;
            var webRoot = "";
            try
            {
                if (!url.Contains("/"))
                    return;
                baseURL = url.Substring(0, url.IndexOf("/"));
                webRoot = url.Substring(url.IndexOf("/"));
            }
            catch (Exception)
            {
                // ignored
            }

            GenericSetup.SaveSettings(https, tray, baseURL, webRoot, Version, company, rootLog, Location);
        }

        private static void PrintBanner()
        {
            Console.WriteLine("                                            ");
            Console.WriteLine("      ..#######:.    ..,#,..     .::##::.   ");
            Console.WriteLine(" .:######          .:;####:......;#;..      ");
            Console.WriteLine(" ...##...        ...##;,;##::::.##...       ");
            Console.WriteLine("    ,#          ...##.....##:::##     ..::  ");
            Console.WriteLine("    ##    .::###,,##.   . ##.::#.:######::. ");
            Console.WriteLine(" ...##:::###::....#. ..  .#...#. #...#:::.  ");
            Console.WriteLine(" ..:####:..    ..##......##::##  ..  #      ");
            Console.WriteLine("     #  .      ...##:,;##;:::#: ... ##..    ");
            Console.WriteLine("    .#  .       .:;####;::::.##:::;#:..     ");
            Console.WriteLine("     #                     ..:;###..        ");
            Console.WriteLine("                                            ");
            Console.WriteLine(" ###########################################");
            Console.WriteLine(" #     FOG                                 #");
            Console.WriteLine(" #     Free Computer Imaging Solution      #");
            Console.WriteLine(" #                                         #");
            Console.WriteLine(" #     http://www.fogproject.org/          #");
            Console.WriteLine(" #                                         #");
            Console.WriteLine(" #     Credits:                            #");
            Console.WriteLine(" #     http://fogproject.org/Credits       #");
            Console.WriteLine(" #     GNU GPL Version 3                   #");
            Console.WriteLine(" ###########################################");
            Console.WriteLine(" #           FOG SERVICE INSTALLER         #");

        }

        private static void InteractiveMode()
        {
            PrintBanner();

            var https = "0";
            var tray = "0";
            var company = "FOG";
            var rootLog = "0";

            Console.Write("Enter your FOG Server address:");
            var server = Console.ReadLine();

            Console.Write("Do you want to enable FOG Tray? [y/n]:");
            if (Console.ReadLine().Trim().ToLower().Equals("y"))
                tray = "1";

            Console.Write("Enter the FOG webroot used [example: /fog]:");
            var webRoot = Console.ReadLine();

            Console.WriteLine("Installing....");

            GenericSetup.SaveSettings(https, tray, server, webRoot, Version, company, rootLog, Location);
        }

        private static void AdjustPermissions()
        {
            var logLocation = Path.Combine(Location, "fog.log");

            if (!File.Exists(logLocation))
                File.Create(logLocation);

            ProcessHandler.Run("chmod", "755 " + logLocation);

            if (Settings.OS != Settings.OSType.Linux)
                return;

            ProcessHandler.Run("chmod", "755 /etc/init.d/FOGService");
            ProcessHandler.Run("systemctl", "enable FOGService >/ dev / null 2 > &1");
            ProcessHandler.Run("sysv-rc-conf", "FOGService on >/ dev / null 2 > &1");
        }

        private static void ExtractFiles()
        {
            var tmpLocation = Path.Combine("/opt/", "FOGService.zip");
            ExtractResource("FOGService.zip", tmpLocation);
            ZipFile.ExtractToDirectory(tmpLocation, Location);
            File.Delete(tmpLocation);

            if (Settings.OS != Settings.OSType.Linux)
                return;

           ExtractResource("init-d", "/etc/init.d/FOGService");
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
