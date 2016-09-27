﻿/*
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
using FOG.Modules.HostnameChanger;
using FOG.Modules.PowerManagement;
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
        private const int MinPrompt = 60;
        private const int MaxPrompt = 60*60*2;
        private const int MaxSleepTime = 60*60*2; // 2 Hours

        private IModule[] _modules;

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
                    Settings.Set("ServerVersion", srvVersion);

                    var alo = response.GetSubResponse("autologout");
                    Settings.Set("ALOTime", (alo == null) ? "0" : alo.GetField("time"));

                    var pDefault = response.GetSubResponse("printermanager");
                    Settings.Set("DefaultPrinter", (pDefault == null) ? "" : pDefault.GetField("default"));

                    var display = response.GetSubResponse("displaymanager");
                    Settings.Set("DisplayX", (display == null || display.Error) ? "" : display.GetField("x"));
                    Settings.Set("DisplayY", (display == null || display.Error) ? "" : display.GetField("y"));
                    Settings.Set("DisplayR", (display == null || display.Error) ? "" : display.GetField("r"));
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
            Bus.Mode = Bus.Role.Server;

            dynamic json = new JObject();
            json.action = "load";
            Bus.Emit(Bus.Channel.Status, json, true);

            if (Settings.OS == Settings.OSType.Linux)
                UserServiceSpawner.Start();
        }

        private void CleanTmpFolder()
        {
            var updatingFile = Path.Combine(Settings.Location, "updating.info");
            if (File.Exists(updatingFile))
                File.Delete(updatingFile);

            try
            {
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
            if (_modules != null)
                return _modules;

            Log.Entry(Name, "Initializing modules");
            var upgradeFiles = new string[] {"FOGUpdateHelper.exe", "FOGUpdateWaiter.exe"};
            _modules = new IModule[]
            {
                new ClientUpdater(upgradeFiles), 
                new TaskReboot(),
                new HostnameChanger(),
                new SnapinClient(),
                new PrinterManager(),
                new PowerManagement(),
                new UserTracker()
            };

            return _modules;
        }

        protected override void ModuleLooper()
        {
            CleanTmpFolder();
            JITCompile();
            Authenticate();

            base.ModuleLooper();

            if (Power.State == Power.Status.Updating)
                UpdateHandler.BeginUpdate();

            Process.GetCurrentProcess().Kill();
        }

        private void JITCompile()
        {
            Log.Entry(Name, "Invoking early JIT compilation on needed binaries");
            ProcessHandler.RunClientEXE("FOGShutdownGUI.exe", "jit");
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

            var promptTime = SandBoxParse(response, "promptTime", MinPrompt, MaxPrompt, MinPrompt);
            Settings.Set("PromptTime", promptTime.ToString());
            var sleep = SandBoxParse(response, "sleep", MinSleepTime, MaxSleepTime, DefaultSleepTime);
            Settings.Set("Sleep", sleep.ToString());

            return sleep;
        }

        private int SandBoxParse(Response response, string setting, int min, int max, int fallback)
        {
            if (response.IsFieldValid(setting))
            {
                int value;
                var success = int.TryParse(response.GetField(setting), out value);

                if (success && value >= min && value <= max)
                {
                    return value;
                }
                else
                {
                    Log.Error(Name, $"Invalid {setting}, using default");
                    return fallback;
                }

            }
            else
            {
                Log.Error(Name, $"Invalid {setting}, using default");
                return fallback;
            }
        }
    }
}
