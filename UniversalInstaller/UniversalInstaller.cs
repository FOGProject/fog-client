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

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Zazzles;
using Mono.Options;

namespace FOG
{
    class UniversalInstaller
    {
        private const string LogName = "Installer";
        private const string DEFAULT_SERVER = "fogserver";
        private const string DEFAULT_WEBROOT = "/fog";

        public static string LogPath;
        private static ConsoleColor _infoColor = ConsoleColor.Yellow;

        [STAThread]
        static void Main(string[] args)
        {
            Log.Output = Log.Mode.Quiet;
            LogPath = Path.Combine(Settings.Location, "SmartInstaller.log");
            Log.FilePath = LogPath;

            var upgrade = false;
            var uninstall = false;
            var tray = false;
            var start = false;
            var rootLog = false;
            var https = false;
            string server = null;
            string webRoot = null;
            bool relaunchRequired = false;
            var logFile = "";

            var help = false;

            var p = new OptionSet() {
                { "server=|SERVER=", $"the FOG server address, defaults to {DEFAULT_SERVER}",
                  v => server = v },
                { "webroot=|WEBROOT=", $"the FOG server webroot, defaults to {DEFAULT_WEBROOT}",
                  v => webRoot = v },
                { "h|https|HTTPS", "use HTTPS for communication",
                  v => https = v != null },
                { "r|rootlog|ROOTLOG", "store fog.log in the root of the filesystem",
                  v => rootLog = v != null },
                { "s|start", "start the service when complete",
                  v => start = v != null },
                { "t|tray|TRAY", "enable the FOG Tray and notifications",
                  v => tray = v != null },
                { "u|uninstall", "uninstall an existing installation",
                  v => uninstall = v != null },
                { "upgrade",  "upgrade an existing installation",
                  v => upgrade = v != null },
                { "l=|log=",  "the log file to use",
                  v => logFile = v },
                { "?|help",  "show this message and exit",
                  v => help = v != null },
            };

            p.Parse(args);

            if (help)
            {
                ShowHelp(p);
                return;
            }

            // Defaults
            if (server == null && !upgrade)
                server = DEFAULT_SERVER;
            if (webRoot == null && !upgrade)
                webRoot = DEFAULT_WEBROOT;

            // If no server was set, show help
            if (string.IsNullOrWhiteSpace(server) && !upgrade)
            {
                ShowHelp(p);
                return;
            }
            // A blank webroot is equal to /
            if (string.IsNullOrWhiteSpace(webRoot) && !upgrade)
            {
                webRoot = DEFAULT_WEBROOT;
            }

            if (!string.IsNullOrWhiteSpace(logFile))
            {
                LogPath = logFile;
                Log.FilePath = LogPath;
                Log.Output = Log.Mode.File;
            }

            if (args.Length == 1)
            {
                if (args[0].Equals("upgrade"))
                    upgrade = true;
                else if (args[0].Equals("uninstall"))
                    uninstall = true;
            }

            //Preliminary environment checks
            if (Settings.OS == Settings.OSType.Mac)
                relaunchRequired =  (Environment.UserName != "root" || Environment.GetEnvironmentVariable("HOME") != "/var/root");


            if (relaunchRequired)
                RelaunchAsRoot(args);
            else if (uninstall)
                PerformCLIUninstall();
            else if (upgrade)
                PerformUpgrade(https ? "1" : "", tray ? "1" : "", server, webRoot, rootLog ? "1" : "");
            else if (args.Length == 0)
                InteractiveMode();
            else
            {
                PrintBanner();
                PrintLicense();
                PrintInfo();
                Install(https ? "1" : "0", tray ? "1" : "0", server, webRoot, "FOG", rootLog ? "1" : "0");
            }
        }

        private static void RelaunchAsRoot(string[] args)
        {
            /* 
            Need to run as the user 'root' so that the certificate store
            is placed in (/var)/root/.mono until the location conflict 
            with macOS System Integrity Protection (SIP) is resolved.
            */
            string cliargs = "";
            System.Diagnostics.Process pRelaunch = new System.Diagnostics.Process();
            pRelaunch.StartInfo.FileName = "/usr/bin/sudo";

            foreach (string arg in args)
            {
                cliargs += " " + arg;
            }

            pRelaunch.StartInfo.Arguments = "-i mono " + System.Reflection.Assembly.GetEntryAssembly().Location + cliargs;

            Console.WriteLine("OS: " + Settings.OS.ToString());
            Console.WriteLine("Current User: " + Environment.UserName + "  Required User: root");
            Console.WriteLine("Current $HOME: " + Environment.GetEnvironmentVariable("HOME") + "  Required $HOME: /var/root");
            Console.WriteLine("Relaunching installer into 'root' user environment...");
            Console.WriteLine("Launching: " + pRelaunch.StartInfo.FileName + ' ' + pRelaunch.StartInfo.Arguments);
            Console.WriteLine("You may be prompted for credentials.");

            pRelaunch.Start();
            pRelaunch.WaitForExit();
            pRelaunch.Dispose();

        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: mono SmartInstaller.exe [OPTIONS]");
            Console.WriteLine("Install the FOG Service onto this computer.");
            Console.WriteLine();
            Console.WriteLine("Special Commands:");
            Console.WriteLine("mono SmartInstaller.exe uninstall");
            Console.WriteLine("mono SmartInstaller.exe upgrade");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);


        }

        private static void PerformUpgrade(string https, string tray, string server, string webRoot, string rootLog)
        {
            Log.Output = Log.Mode.File;
            var settingsFile = Path.Combine(Settings.Location, "settings.json");
            Settings.SetPath(settingsFile);
            if (string.IsNullOrEmpty(https)) https = Settings.Get("HTTPS");
            if (string.IsNullOrEmpty(tray)) tray = Settings.Get("Tray");
            if (string.IsNullOrEmpty(server)) server = Settings.Get("Server");
            if (string.IsNullOrEmpty(webRoot)) webRoot = Settings.Get("WebRoot");
            if (string.IsNullOrEmpty(rootLog)) rootLog = Settings.Get("RootLog");
            var lastInstallDir = Path.GetDirectoryName(Settings.Location);

            if (!Install(https, tray, server, webRoot, Settings.Get("Company"), rootLog, false, lastInstallDir))
            {
                var installLog = File.ReadAllLines(Log.FilePath);
                Log.FilePath = Path.Combine(Settings.Location, "fog.log");
                if (Settings.Get("RootLog").Equals("1") && Settings.OS == Settings.OSType.Windows)
                    Log.FilePath = @"C:\fog.log";
                foreach (var line in installLog)
                {
                    Log.Entry(LogName, line);
                }
                Environment.Exit(1);
            }

            File.Copy(settingsFile, 
                Path.Combine(Helper.Instance.GetLocation(), "settings.json"), true);
            File.Copy(Path.Combine(Settings.Location, "token.dat"), 
                Path.Combine(Helper.Instance.GetLocation(), "token.dat"), true);
        }

        private static void PrintBanner()
        {
            var bannerColor = ConsoleColor.White;
            var paddingSize = (Log.HeaderLength - 48)/2;
            var builder = new StringBuilder();
            for (var i = 0; i < paddingSize; i++)
            {
                builder.Append(" ");
            }
            var padding = builder.ToString();

            Log.NewLine();
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
            
            Log.Write("Webroot [default: /fog]:                 ");
            Console.ForegroundColor = _infoColor;
            var webRoot = Console.ReadLine();
            Console.ResetColor();
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = "/fog";
            Log.Write("Enable tray icon? [Y/n]:                 ");
            Console.ForegroundColor = _infoColor;
            var rawTray = Console.ReadLine();
            Console.ResetColor();
            if (rawTray.Trim().Equals("n", StringComparison.OrdinalIgnoreCase))
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
                Log.Write("Start FOG Service when done? [Y/n]:      ");
                Console.ForegroundColor = _infoColor;
                var rawStart = Console.ReadLine();
                Console.ResetColor();
                if (rawStart.Trim().Equals("n", StringComparison.OrdinalIgnoreCase))
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
            Log.WriteLine($"See {Log.FilePath} for more information.", _infoColor);
            Log.NewLine();
        }

        private static void PrintLicense()
        {
            Log.Header("License");
            Log.NewLine();
            Log.WriteLine("FOG Service Copyright (C) 2014-2023 FOG Project", _infoColor);
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
            string webRoot, string company, string rootLog, bool skipSave= false, string location = null)
        {
            Log.NewLine();
            Log.Header("Installing");
            Log.NewLine();

            if (!DoAction("Getting things ready", Helper.Instance.PrepareFiles))
                return false;
            if (!DoAction("Installing files",() => Helper.Instance.Install(https, tray, server, webRoot, company, rootLog, location)))
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
