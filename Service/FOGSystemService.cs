﻿/*
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
using System.Diagnostics;
<<<<<<< HEAD
using System.IO;
using System.Threading;
=======
using System.Net.Security;
using System.Threading;
using FOG.Handlers;
using FOG.Handlers.Middleware;
using FOG.Handlers.Power;
using FOG.Modules;
using FOG.Modules.ClientUpdater;
>>>>>>> refs/remotes/FOGProject/v0.9.x
using FOG.Modules.GreenFOG;
using FOG.Modules.HostnameChanger;
using FOG.Modules.PrinterManager;
using FOG.Modules.SnapinClient;
using FOG.Modules.TaskReboot;
using FOG.Modules.UserTracker;
using Newtonsoft.Json.Linq;
using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;
using Zazzles.Modules.Updater;

namespace FOG
{
    public class FOGSystemService : AbstractService
    {
        public FOGSystemService()
        {
            Log.FilePath = Path.Combine(Settings.Location, "fog.log");
            Bus.SetMode(Bus.Mode.Server);
        }

        protected override void Load()
        {
            // Kill any existing sub-processes
            ProcessHandler.KillAllEXE("FOGUserService");
            ProcessHandler.KillAllEXE("FOGTray");

            dynamic json = new JObject();
            json.action = "load";
            Bus.Emit(Bus.Channel.Status, json, true);
<<<<<<< HEAD

            // Start the UserServiceSpawner
            if (Settings.OS == Settings.OSType.Linux)
                UserServiceSpawner.Start();
=======
>>>>>>> refs/remotes/FOGProject/v0.9.x
        }

        protected override void Unload()
        {
            UserServiceSpawner.Stop();

            dynamic json = new JObject();
            json.action = "unload";
            Bus.Emit(Bus.Channel.Status, json, true);
            Bus.Dispose();

            // Kill the sub-processes
            UserServiceSpawner.KillAll();
            ProcessHandler.KillAllEXE("FOGUserService");
            ProcessHandler.KillAllEXE("FOGTray");
        }

        protected override AbstractModule[] GetModules()
        {
            var upgradeFiles = new string[] {"FOGUpdateHelper.exe", "FOGUpdateWaiter.exe"};
            return new AbstractModule[]
            {
                new ClientUpdater(upgradeFiles), 
                new TaskReboot(),
                new HostnameChanger(),
                new SnapinClient(),
                new PrinterManager(),
                new GreenFOG(),
                new UserTracker()
            };
        }

        protected override void ModuleLooper()
        {
            Authenticate();

            base.ModuleLooper();

            if (Power.Updating)
                UpdateHandler.BeginUpdate();

            Process.GetCurrentProcess().Kill();
        }

        private void Authenticate()
        {
            while (true)
            {
                Log.NewLine();
                Log.PaddedHeader("Authentication");
                Log.Entry("Client-Info", string.Format("Version: {0}", Settings.Get("Version")));
                if (Authentication.HandShake()) break;

                Log.Entry(Name, "Sleeping for 120 seconds");
                Thread.Sleep(120 * 1000);
            }
            Log.NewLine();
        }

        private void Authenticate()
        {
            while (true)
            {
                Log.NewLine();
                Log.PaddedHeader("Authentication");
                Log.Entry("Client-Info", string.Format("Version: {0}", RegistryHandler.GetSystemSetting("Version")));
                if(Authentication.HandShake()) break;


                Log.Entry(Name, "Sleeping for 120 seconds");
                Thread.Sleep(120 * 1000);
            }
            Log.NewLine();
        }

        protected override int? GetSleepTime()
        {
            var response = Communication.GetResponse("/management/index.php?node=client&sub=configure");

            if (response.Error) return null;

            // Set the shutdown graceperiod
            Settings.Set("gracePeriod", response.GetField("#promptTime"));

            if (!response.IsFieldValid("#sleep")) return null;

            try
            {
                var sleepTime = int.Parse(response.GetField("#sleep"));
                if (sleepTime >= DefaultSleepTime)
                {
                    Settings.Set("Sleep", sleepTime.ToString());
                    return sleepTime;
                }

                Log.Entry(Name,
                    $"Sleep time set on the server is below the minimum of {DefaultSleepTime}");
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Unable to parse sleep time");
                Log.Error(Name, ex);
            }
            return null;
        }
    }
}
