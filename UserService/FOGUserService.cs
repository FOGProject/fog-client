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
using FOG.Modules.AutoLogOut;
using FOG.Modules.DisplayManager;
using FOG.Modules.PrinterManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zazzles;
using Zazzles.Data;
using Zazzles.Middleware;
using Zazzles.Modules;

namespace FOG
{
    internal class FOGUserService : AbstractService
    {
        private IModule[] _modules;

        private static void OnUpdate(dynamic data)
        {
            if (data.action == null) return;
            Log.Debug("User Service", data.ToString());

            if (!data.action.ToString().Equals("start")) return;

            if (Settings.OS == Settings.OSType.Windows)
            {
                UpdateWaiterHelper.SpawnUpdateWaiter(System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            else if (Settings.OS == Settings.OSType.Mac)
            {
                UpdateWaiterHelper.SpawnUpdateWaiter("launchctl", "load -w /Library/LaunchAgents/org.freeghost.useragent.plist");
            }

            Power.Updating = true;
            Environment.Exit(0);
        }

        private static void OnPower(dynamic data)
        {
            if (data.action == null) return;
            string action = data.action.ToString();

            if (action.Trim().Equals("request"))
                ShutdownNotification(data);
        }

        protected override IModule[] GetModules()
        {
            if (_modules != null)
                return _modules;

            Log.Entry(Name, "Initializing modules");

            _modules =  new IModule[]
            {
                new AutoLogOut(),
                new DefaultPrinterManager(),
                new DisplayManager()
            };

            return _modules;
        }

        protected override Response GetLoopData()
        {
            try
            {
                Settings.Reload();

                int alo;
                int displayX;
                int displayY;
                int displayR;
                string printer;

                int.TryParse(Settings.Get("ALOTime"), out alo);
                printer = Settings.Get("DefaultPrinter");
                int.TryParse(Settings.Get("DisplayX"), out displayX);
                int.TryParse(Settings.Get("DisplayY"), out displayY);
                int.TryParse(Settings.Get("DisplayR"), out displayR);

                var data = new JObject
                {
                    ["autologout"] = new JObject { ["time"] = alo },
                    ["defaultprintermanager"] = new JObject { ["name"] = printer },
                    ["displaymanager"] = new JObject { ["error"] = ((displayX == -1) ? "Configuration not set" : "ok"),
                        ["x"] = displayX,
                        ["y"] = displayY,
                        ["r"] = displayR }

                };
                return new Response(data, false);
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
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);
            Bus.Subscribe(Bus.Channel.Power, OnPower);
            Bus.Subscribe(Bus.Channel.Status, OnStatus);
        }

        private void OnStatus(dynamic data)
        {
            if (data.action == null) return;
            if (data.action.toString().Equals("unload"))
                Environment.Exit(0);
        }

        protected override void Unload()
        {
        }

        protected override int? GetSleepTime()
        {
            try
            {
                var sleepTimeStr = Settings.Get("Sleep");
                var sleepTime = int.Parse(sleepTimeStr);
                if (sleepTime >= MIN_SLEEP_TIME)
                    return sleepTime;

                Log.Entry(Name,
                    $"Sleep time set on the server is below the minimum of {MIN_SLEEP_TIME}");
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Unable to parse sleep time");
                Log.Error(Name, ex);
            }

            return null;
        }

        private static void ShutdownNotification(dynamic data)
        {
            Log.Entry("Service", "Prompting user");
            string jsonData = JsonConvert.SerializeObject(data);

            ProcessHandler.RunClientEXE("FOGShutdownGUI.exe", Transform.EncodeBase64(jsonData), false);
        }
    }
}
