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
using Zazzles;

namespace FOG
{
    class UniversalInstaller
    {
        private const string LogName = "Installer";

        [STAThread]
        static void Main(string[] args)
        {
            Log.Output = Log.Mode.Quiet;

            if (args.Length == 5)
                ProcessArgs(args);
            else if(args.Length == 1 && args[1].Equals("uninstall"))
                PerformCLIUninstall();
            else
                InteractiveMode();
        }

        private static void ProcessArgs(string[] args)
        {
            var url = args[0];
            var tray = args[1];
            var company = args[2];
            var rootLog = args[3];
            var https = args[4];

            var server = url;
            var webRoot = "";
            try
            {
                if (!url.Contains("/"))
                    return;
                server = url.Substring(0, url.IndexOf("/"));
                webRoot = url.Substring(url.IndexOf("/"));
            }
            catch (Exception)
            {
                // ignored
            }

            Install(https, tray, server, webRoot, company, rootLog);

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
            if (Settings.OS == Settings.OSType.Windows)
            {
                Helper.Instance.PrepareFiles();
                Helper.Instance.Install();
            }
            else
                PerformCLIInstall();
        }

/*        private static void ShowGUI()
        {
            Log.Output = Log.Mode.File;
            Log.FilePath = Path.Combine(Settings.Location, "FOGService-install.log");

            Application.EnableVisualStyles();
            var gui = new GUI();
            Application.Run(gui);

            if(!gui.Success)
                Environment.Exit(1);
        }
*/

        private static void PerformCLIUninstall()
        {
            Log.Output = Log.Mode.Console;
            PrintBanner();
            Helper.Instance.Uninstall();
        }

        private static void PerformCLIInstall()
        {
            Log.Output = Log.Mode.Console;

            PrintBanner();
            Console.WriteLine("");
            Console.WriteLine("By installing this software you agree to the GPL v3 license");
            Console.WriteLine("");

            var https = "0";
            var tray = "0";
            var company = "FOG";
            var rootLog = "0";

            Console.Write("Enter your FOG Server address:");
            var server = Console.ReadLine();

            Console.Write("Enter the FOG webroot used [example: /fog]:");
            var webRoot = Console.ReadLine();

            Install(https, tray, server, webRoot, company, rootLog);     
        }

        private static void Install(string https, string tray, string server, string webRoot, string company, string rootLog)
        {
            Console.WriteLine("Getting things ready...");
            Helper.Instance.PrepareFiles();

            Console.WriteLine("Installing files...");
            Helper.Instance.Install(https, tray, server, webRoot, company, rootLog);

            Console.WriteLine("Applying Configuration...");
            Helper.SaveSettings(https, tray, server, webRoot, company, rootLog);
            Helper.Instance.Configure();
            
            Console.WriteLine("Setting Up Encrypted Tunnel...");
            Helper.PinServerCert();
            Helper.PinFOGCert();
        }
    }
}
