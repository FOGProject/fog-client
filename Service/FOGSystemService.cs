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
using System.Diagnostics;
using System.IO;
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
        protected override Response GetLoopData()
        {

            try
            {
                var response = Communication.GetResponse("/management/index.php?sub=requestClientInfo", true);
                // Construct the clientupdater data regardless of encryption
                var srvClientVersion = Communication.GetRawResponse("/service/getversion.php?clientver");
                var srvVersion = Communication.GetRawResponse("/service/getversion.php");

                var clientUpdaterData = new JObject { ["version"] = srvClientVersion };
                response.Data["clientupdater"] = clientUpdaterData;

                Log.NewLine();
                Log.Entry(Name, "Creating user agent cache");
                try
                {
                    Settings.Set("server-version", srvVersion);

                    // Dump user-service configuration to the settings file
                    var alo = response.GetSubResponse("autologout");
                    Settings.Set("alo-time", (alo == null) ? "0" : alo.GetField("time"));

                    var pDefault = response.GetSubResponse("printermanager");
                    Settings.Set("alo-time", (pDefault == null) ? "" : pDefault.GetField("default"));
                }
                catch (Exception ex)
                {
                    Log.Error(Name, "Unable to set user agent cache");
                    Log.Error(Name, ex);
                }


                return response;
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Unable to get cycle data");
                Log.Error(Name, ex);
            }


            return new Response();
        }

        protected override void Load()
        {

            try
            {
                // Kill any existing sub-processes
                ProcessHandler.KillAllEXE("FOGUserService");
                ProcessHandler.KillAllEXE("FOGTray");

                // Delete any tmp files from last session
                var tmpDir = Path.Combine(Settings.Location, "tmp");
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not clear last session data");
                Log.Error(Name, ex);
            }
            
            // Update the Runtime variable
            var runtimeFile = Path.Combine(Settings.Location, "runtime");

            if (File.Exists(runtimeFile))
            {
                var runtime = File.ReadAllText(runtimeFile).Trim();
                Settings.Set("Runtime", runtime);
            }

            Bus.SetMode(Bus.Mode.Server);

            dynamic json = new JObject();
            json.action = "load";
            Bus.Emit(Bus.Channel.Status, json, true);

            if (Settings.OS == Settings.OSType.Linux)
                UserServiceSpawner.Start();
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

        protected override IModule[] GetModules()
        {
            var upgradeFiles = new string[] {"FOGUpdateHelper.exe", "FOGUpdateWaiter.exe"};
            return new IModule[]
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
            var maxTries = 5;
            for(int i = 0; i < maxTries; i ++)
            {
                Log.NewLine();
                Log.PaddedHeader("Authentication");
                Log.Entry("Client-Info", $"Version: {Settings.Get("Version")}");
                Log.Entry("Client-Info", $"OS:      {Settings.OS}");

                if (Authentication.HandShake()) break;
            }
            Log.NewLine();
        }

        protected override int? GetSleepTime()
        {
            Response response;
            try
            {
                response = Communication.GetResponse("/management/index.php?sub=requestClientInfo&configure");
                if (response.Error) return null;
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Unable to retrieve configuration");
                Log.Error(Name, ex);
                return null;
            }

            Settings.Set("gracePeriod", response.GetField("promptTime"));
            if (!response.IsFieldValid("sleep")) return null;

            try
            {
                var sleepTime = int.Parse(response.GetField("sleep"));
                if (sleepTime >= MinSleepTime)
                {
                    Settings.Set("Sleep", sleepTime.ToString());
                    return sleepTime;
                }

                Log.Entry(Name,
                    $"Sleep time set on the server is below the minimum of {MinSleepTime}");
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
