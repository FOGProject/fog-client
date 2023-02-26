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
using Zazzles;

namespace FOG
{
    /// <summary>
    ///     Coordinate all user specific FOG modules
    /// </summary>
    internal class main
    {
        private const string LogName = "UserService";
        private static AbstractService _fogService;

        public static void Main(string[] args)
        {
            Log.Output = Log.Mode.Quiet;
            Log.FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fog_user.log");
            Log.Output = Log.Mode.File;

            AppDomain.CurrentDomain.UnhandledException += Log.UnhandledException;
            Eager.Initalize();

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "updating.info")))
            {
                Log.Entry(LogName, "Update.info found, exiting program");
                UpdateWaiterHelper.SpawnUpdateWaiter(Settings.Location);
                Environment.Exit(0);
            }

            _fogService = new FOGUserService();
            _fogService.Start();

            if (Settings.Get("Tray").Equals("1"))
                StartTray();
        }

        private static void StartTray()
        {
            switch (Settings.OS)
            {
                case Settings.OSType.Windows:
                    ProcessHandler.RunClientEXE("FOGTray.exe", "", false);
                    break;
                case Settings.OSType.Mac:
                    ProcessHandler.Run("open", Path.Combine(Settings.Location, "OSX-FOG-TRAY.app"), false);
                    break;
            }
        }
    }
}