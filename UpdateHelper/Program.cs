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
using FOG.Handlers;
using Newtonsoft.Json.Linq;

namespace FOG
{
    internal class Program
    {
        private const string LogName = "UpdateHelper";
        private static IUpdate _instance;

        public static void Main(string[] args)
        {
            Eager.Initalize();

            if (args.Length > 0)
            {
                Log.FilePath = args[0];
                Log.Output = Log.Mode.File;
            }
            switch (Settings.OS)
            {
                case Settings.OSType.Mac:
                    _instance = new MacUpdate();
                    break;
                case Settings.OSType.Linux:
                    _instance = new LinuxUpdate();
                    break;
                default:
                    _instance = new WindowsUpdate();
                    break;
            }

            try
            {
                Log.Entry(LogName, "Shutting down service...");
                _instance.StopService();

                Log.Entry(LogName, "Killing remaining FOG processes...");
                ProcessHandler.KillAllEXE("FOGService");
                Log.Entry(LogName, "Applying installer...");
                _instance.ApplyUpdate();

                var parentDir = Directory.GetParent(Settings.Location).ToString();

                if (File.Exists(Path.Combine(Settings.Location, "settings.json")))
                    File.Copy(Path.Combine(Settings.Location, "settings.json"), Path.Combine(parentDir, "settings.json"),
                        true);

                if (File.Exists(Path.Combine(Settings.Location, "token.dat")))
                    File.Copy(Path.Combine(Settings.Location, "token.dat"), Path.Combine(parentDir, "token.dat"), true);

                //Start the service

                Log.Entry(LogName, "Starting service...");
                _instance.StartService();

                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updating.info")))
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updating.info"));
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not perform update!");
                Log.Error(LogName, ex);
            }
        }
    }
}