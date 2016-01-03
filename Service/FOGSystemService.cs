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


using System.Collections.Generic;
using System.IO;
using System.Threading;
using FOG.Modules.GreenFOG;
using FOG.Modules.HostnameChanger;
using FOG.Modules.PrinterManager;
using FOG.Modules.SnapinClient;
using FOG.Modules.TaskReboot;
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

            Bus.Emit(Bus.Channel.Status, new JObject { { "action", "load" } }, true);

            // Start the UserServiceSpawner
            if (Settings.OS == Settings.OSType.Linux)
                UserServiceSpawner.Start();
        }

        protected override void Unload()
        {
            UserServiceSpawner.Stop();

            Bus.Emit(Bus.Channel.Status, new JObject { { "action", "unload" } }, true);
            Bus.Dispose();

            // Kill the sub-processes
            UserServiceSpawner.KillAll();
            ProcessHandler.KillAllEXE("FOGUserService");
            ProcessHandler.KillAllEXE("FOGTray");
        }

        protected override Dictionary<string, IEventProcessor> GetModules()
        {
            var upgradeFiles = new[] {"FOGUpdateHelper.exe", "FOGUpdateWaiter.exe"};
            var updater = new ClientUpdater(upgradeFiles);
            var taskReboot = new TaskReboot();
            var hostnameChanger = new HostnameChanger();
            var snapinClient = new SnapinClient();
            var printerManager = new PrinterManager();
            var greenFOG = new GreenFOG();

            return new Dictionary<string, IEventProcessor>
            {
                {updater.Name, updater},
                {taskReboot.Name, taskReboot},
                {hostnameChanger.Name, hostnameChanger},
                {snapinClient.Name, snapinClient},
                {printerManager.Name, printerManager},
                {greenFOG.Name, greenFOG},
            };
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
    }
}
