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
using FOG.Modules.AutoLogOut;
using FOG.Modules.PrinterManager;
using Newtonsoft.Json;
using Zazzles;
using Zazzles.Data;
using Zazzles.Modules;

namespace FOG
{
    internal class FOGUserService : AbstractService
    {
        public FOGUserService()
        {
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);
            Bus.Subscribe(Bus.Channel.Power, OnPower);
        }

        private static void OnUpdate(dynamic data)
        {
            if (data.action == null) return;
            Log.Entry("User Service", data.ToString());

            if (!data.action.ToString().Equals("start")) return;

            if (Settings.OS != Settings.OSType.Linux)
            {
                Log.Entry("User Service", "Spawning waiter");
                UpdateWaiterHelper.SpawnUpdateWaiter(Settings.Location);
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

        protected override AbstractModule[] GetModules()
        {
            return new AbstractModule[]
            {
                new AutoLogOut(),
                //new DefaultPrinterManager()
            };
        }

        protected override void Load()
        {
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
                if (sleepTime >= DefaultSleepTime)
                    return sleepTime;

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

        private static void ShutdownNotification(dynamic data)
        {
            Log.Entry("Service", "Prompting user");
            string jsonData = JsonConvert.SerializeObject(data);

            ProcessHandler.RunClientEXE("FOGShutdownGUI.exe", Transform.EncodeBase64(jsonData), false);
        }
    }
}