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
using System.Text;
using System.Windows.Forms;
using Zazzles;

namespace FOG
{
    class UniversalInstaller
    {
        private const string LogName = "Installer";
        public static string LogPath;
        private static ConsoleColor _infoColor = ConsoleColor.Yellow;

        [STAThread]
        static void Main(string[] args)
        {
            Log.Output = Log.Mode.Quiet;
            LogPath = Path.Combine(Settings.Location, "SmartInstaller.log");
            Log.FilePath = LogPath;

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
            Log.Output = Log.Mode.File;
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
            var bannerColor = ConsoleColor.White;
            var paddingSize = (Log.HeaderLength - 48)/2;
            var builder = new StringBuilder();
            for (int i = 0; i < paddingSize; i++)
            {
                builder.Append(" ");
            }
            var padding = builder.ToString();

            Log.WriteLine(padding + "                                            ", bannerColor);
            Log.WriteLine(padding + "      ..#######:.    ..,#,..     .::##::.   ", bannerColor);
            Log.WriteLine(padding + " .:######          .:;####:......;#;..      ", bannerColor);
            Log.WriteLine(padding + " ...##...        ...##;,;##::::.##...       ", bannerColor);
            Log.WriteLine(padding + "    ,#          ...##.....##:::##     ..::  ", bannerColor);
            Log.WriteLine(padding + "    ##    .::###,,##.   . ##.::#.:######::. ", bannerColor);
            Log.WriteLine(padding + " ...##:::###::....#. ..  .#...#. #...#:::.  ", bannerColor);
            Log.WriteLine(padding + " ..:####:..    ..##......##::##  ..  #      ", bannerColor);
            Log.WriteLine(padding + "     #  .      ...##:,;##;:::#: ... ##..    ", bannerColor);
            Log.WriteLine(padding + "    .#  .       .:;####;::::.##:::;#:..     ", bannerColor);
            Log.WriteLine(padding + "     #                     ..:;###..        ", bannerColor);
            Log.WriteLine(padding + "                                            ", bannerColor);
            Log.WriteLine(padding + " ###########################################", bannerColor);
            Log.WriteLine(padding + " #     FOG                                 #", bannerColor);
            Log.WriteLine(padding + " #     Free Computer Imaging Solution      #", bannerColor);
            Log.WriteLine(padding + " #                                         #", bannerColor);
            Log.WriteLine(padding + " #     https://www.fogproject.org/         #", bannerColor);
            Log.WriteLine(padding + " #                                         #", bannerColor);
            Log.WriteLine(padding + " #     Credits:                            #", bannerColor);
            Log.WriteLine(padding + " #     https://fogproject.org/Credits      #", bannerColor);
            Log.WriteLine(padding + " #     GNU GPL Version 3                   #", bannerColor);
            Log.WriteLine(padding + " ###########################################", bannerColor);
            Log.WriteLine(padding + " #           FOG Service Installer         #", bannerColor);
            Log.NewLine();
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


        private static void PerformCLIUninstall(bool noInfo = false)
        {
            if (!noInfo)
            {
                Log.FilePath = LogPath;
                Log.Output = Log.Mode.Console;

                PrintBanner();
                PrintLicense();
                PrintInfo();
            }

            Log.Header("Uninstall");
            Log.NewLine();

            DoAction("Uninstalling", Helper.Instance.Uninstall);
            Log.NewLine();
        }

        private static void PerformCLIInstall(bool noInfo = false)
        {
            if (!noInfo)
            {
                Log.FilePath = LogPath;
                Log.Output = Log.Mode.Console;

                PrintBanner();
                PrintLicense();
                PrintInfo();
            }
            
            Log.Header("Configure");
            Log.NewLine();

            var https = "0";
            var tray = "1";
            var company = "FOG";
            var rootLog = "0";

            Log.Write("FOG Server address [default: fogserver]: ");
            Console.ForegroundColor = _infoColor;
            var server = Console.ReadLine();
            Console.ResetColor();
            if (string.IsNullOrWhiteSpace(server))
                server = "fogserver";
            
            Log.Write("Webroot [default: /fog]:                  ");
            Console.ForegroundColor = _infoColor;
            var webRoot = Console.ReadLine();
            Console.ResetColor();
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = "/fog";
            Log.Write("Enable tray icon? [Y/n]:                  ");
            Console.ForegroundColor = _infoColor;
            var rawTray = Console.ReadLine();
            Console.ResetColor();
            if (rawTray.Trim().ToLower().Equals("n"))
                tray = "0";

            // Check hostname length (OSX appends .local by default)
            var hostname = Environment.MachineName;
            if (hostname.Length > 15)
            {
                Log.NewLine();
                Log.WriteLine("Your hostname is " + hostname, _infoColor);
                Log.WriteLine("This exceeds the 15 character limit.", _infoColor);
                Log.WriteLine("Auto registration will not run until this is fixed.", _infoColor);
                Log.WriteLine("Press ENTER to proceed with installation.", _infoColor);
                Console.ReadLine();
                Log.NewLine();
            }

            var start = Settings.OS == Settings.OSType.Linux;

            if (start)
            {
                Log.Write("Start FOG Service when done? [Y/n]:       ");
                Console.ForegroundColor = _infoColor;
                var rawStart = Console.ReadLine();
                Console.ResetColor();
                if (rawStart.Trim().ToLower().Equals("n"))
                    start = false;
            }

            if (!Install(https, tray, server, webRoot, company, rootLog))
            {
                Log.NewLine();
                Log.WriteLine("Installation failed, cleaning system", _infoColor);
                Log.NewLine();
                PerformCLIUninstall(true);
            }
            else
            {
                if (start)
                    Start();
            }

            Log.Header("Finished");
            Log.NewLine();
            Log.WriteLine($"See {LogPath} for more information.", _infoColor);
            Log.NewLine();
        }

        private static void PrintLicense()
        {
            Log.Header("License");
            Log.NewLine();
            Log.WriteLine("FOG Service Copyright (C) 2014-2016 FOG Project", _infoColor);
            Log.WriteLine("This program comes with ABSOLUTELY NO WARRANTY.", _infoColor);
            Log.WriteLine("This is free software, and you are welcome to redistribute it under certain", _infoColor);
            Log.WriteLine("conditions. See your FOG server under 'FOG Configuration' -> 'License' for", _infoColor);
            Log.WriteLine("further information.", _infoColor);
            Log.NewLine();
        }

        private static void PrintInfo()
        {
            Log.Header("Information");
            Log.NewLine();
            PrintInfo("Version", Helper.ClientVersion);
            PrintInfo("OS", Settings.OS.ToString());
            PrintInfo("Current Path", Settings.Location);
            PrintInfo("Install Location", Helper.Instance.GetLocation());

            Helper.Instance.PrintInfo();
            Log.NewLine();
        }

        private static bool Install(string https, string tray, string server, 
            string webRoot, string company, string rootLog, bool skipSave= false)
        {
            Log.NewLine();
            Log.Header("Installing");
            Log.NewLine();

            if (!DoAction("Getting things ready", Helper.Instance.PrepareFiles))
                return false;
            if (!DoAction("Installing files",() => Helper.Instance.Install(https, tray, server, webRoot, company, rootLog)))
                    return false;

            if (Settings.OS == Settings.OSType.Windows)
                return true;

            if(!skipSave)
                if (!DoAction("Saving Configuration",() => Helper.SaveSettings(https, tray, server, webRoot, company, rootLog)))
                    return false;

            if (!DoAction("Applying Configuration", Helper.Instance.Configure))
                return false;

            if (!DoAction("Pinning FOG Project", Helper.PinFOGCert))
                return false;

            if (!DoAction("Pinning Server", () => Helper.PinServerCert(Helper.Instance.GetLocation(), skipSave)))
                return false;

            Log.NewLine();
            return true;
        }

        private static bool DoAction(string action, Func<bool> method )
        {
            Log.Action(action);
            Log.Output = Log.Mode.File;
            var success = false;

            try
            {
                success = method();
            }
            catch (Exception ex)
            {               
                Log.Error(LogName, "Method: " + method.Method.Name);
                Log.Error(LogName, ex);
            }

            Log.Output = Log.Mode.Console;
            Log.ActionResult(success);

            return success;
        }

        private static void Start()
        {
            Log.Action("Starting FOG Service");

            Log.Output = Log.Mode.File;
            var controlPath = Path.Combine(Helper.Instance.GetLocation(), "control.sh");

            var returnCode = ProcessHandler.Run("/bin/bash", controlPath + " start");
            Log.Output = Log.Mode.Console;
            Log.ActionResult(returnCode == 0);
            Log.NewLine();
        }

        public static void PrintInfo(string setting, string value)
        {
            var builder = new StringBuilder(setting);

            for (var i = setting.Length; i < Log.HeaderLength - value.Length; i++)
            {
                builder.Append(".");
            }

            Log.Write(builder.ToString());
            Log.WriteLine(value, _infoColor);
        }
    }
}
