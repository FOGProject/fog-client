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
using System.Windows.Forms;
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
            else if (args.Length == 1 && args[0].Equals("uninstall"))
                PerformCLIUninstall();
            else if (args.Length == 1 && args[0].Equals("upgrade"))
                PerformUpgrade();
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

        private static void PerformUpgrade()
        {
            var settingsFile = Path.Combine(Settings.Location, "settings.json");

            Settings.SetPath(settingsFile);
            Settings.Set("Version", Helper.ClientVersion);

            Install(Settings.Get("HTTPS"), Settings.Get("Tray"), Settings.Get("Server"), 
                Settings.Get("WebRoot"), Settings.Get("Company"), Settings.Get("RootLog"), 
                true);

            File.Copy(settingsFile, 
                Path.Combine(Helper.Instance.GetLocation(), "settings.json"), true);
            File.Copy(Path.Combine(Settings.Location, "token.dat"), 
                Path.Combine(Helper.Instance.GetLocation(), "token.dat"), true);

            var runtime = Settings.Get("Runtime");
            if(!string.IsNullOrEmpty(runtime))
                File.WriteAllText(Path.Combine(Helper.Instance.GetLocation(), "runtime"), runtime);
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
            Console.WriteLine(" #     https://www.fogproject.org/         #");
            Console.WriteLine(" #                                         #");
            Console.WriteLine(" #     Credits:                            #");
            Console.WriteLine(" #     https://fogproject.org/Credits      #");
            Console.WriteLine(" #     GNU GPL Version 3                   #");
            Console.WriteLine(" ###########################################");
            Console.WriteLine(" #           FOG SERVICE INSTALLER         #");
        }

        private static void InteractiveMode()
        {
            try
            {
                ShowGUI();
            }
            catch (Exception ex)
            {
                if (Settings.OS == Settings.OSType.Windows)
                {
                    Helper.Instance.PrepareFiles();
                    Helper.Instance.Install();
                    Helper.Instance.Configure();
                }
                else
                {
                    PerformCLIInstall();
                }
            }
        }

        private static void ShowGUI()
        {
            throw new NotImplementedException();

            Log.Output = Log.Mode.File;
            Log.FilePath = Path.Combine(Settings.Location, "FOGService-install.log");

            Application.EnableVisualStyles();
            var gui = new GUI();
            Application.Run(gui);
        }


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
            Console.WriteLine("Version: " + Helper.ClientVersion);
            Console.WriteLine("OS:      " + Settings.OS);
            Console.WriteLine("");

            var https = "0";
            var tray = "0";
            var company = "FOG";
            var rootLog = "0";

            Console.Write("Enter your FOG Server address: ");
            var server = Console.ReadLine();

            Console.Write("Enter the FOG webroot used [example: /fog]: ");
            var webRoot = Console.ReadLine();

            Console.Write("Enable tray icon? [y/n]: ");
            var rawTray = Console.ReadLine();
            if (rawTray.Trim().ToLower().Equals("y"))
                tray = "1";

            Install(https, tray, server, webRoot, company, rootLog);     
        }

        private static void Install(string https, string tray, string server, 
            string webRoot, string company, string rootLog, bool skipSave= false)
        {
            Console.WriteLine("Getting things ready...");
            Helper.Instance.PrepareFiles();

            Console.WriteLine("Installing files...");
            Helper.Instance.Install(https, tray, server, webRoot, company, rootLog);

            Console.WriteLine("Applying Configuration...");
            if(!skipSave)
                Helper.SaveSettings(https, tray, server, webRoot, company, rootLog);
            Helper.Instance.Configure();
            
            Console.WriteLine("Setting Up Encrypted Tunnel...");
            Helper.PinServerCert(Helper.Instance.GetLocation(), skipSave);
            Helper.PinFOGCert();
        }
    }
}
