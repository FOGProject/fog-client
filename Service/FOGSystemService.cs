using System;
using System.Diagnostics;
using FOG.Handlers;
using FOG.Handlers.Middleware;
using FOG.Handlers.Power;
using FOG.Modules;
using FOG.Modules.ClientUpdater;
using FOG.Modules.DisplayManager;
using FOG.Modules.GreenFOG;
using FOG.Modules.HostnameChanger;
using FOG.Modules.SnapinClient;
using FOG.Modules.TaskReboot;
using FOG.Modules.UserTracker;
using Newtonsoft.Json.Linq;

namespace FOG
{
    class FOGSystemService : AbstractService
    {
        public FOGSystemService() : base()
        {
            Bus.SetMode(Bus.Mode.Client);
        }

        protected override void Load()
        {
            Bus.SetMode(Bus.Mode.Client);
            Bus.Emit(Bus.Channel.Status, new JObject { "action", "load" }, true);

            Log.NewLine();
            Log.PaddedHeader("Authentication");
            Log.Entry("Client-Info", string.Format("Version: {0}", RegistryHandler.GetSystemSetting("Version")));
            if (!Authentication.HandShake()) return;
            Log.NewLine();
        }


        protected override void Unload()
        {
            Bus.Emit(Bus.Channel.Status, new JObject { "action", "unload" }, true);
            Bus.Dispose();

            // Kill the sub-processes
            foreach (var process in Process.GetProcessesByName("FOGUserService"))
                process.Kill();

            foreach (var process in Process.GetProcessesByName("FOGTray"))
                process.Kill();
        }

        protected override AbstractModule[] GetModules()
        {
            return new AbstractModule[]
            {
                new ClientUpdater(), 
                new TaskReboot(), 
                new HostnameChanger(), 
                new SnapinClient(), 
                new DisplayManager(), 
                new GreenFOG(), 
                new UserTracker() 
            };
        }


        protected override void ModuleLooper()
        {
            base.ModuleLooper();

            if (Power.Updating)
                UpdateHandler.BeginUpdate();
        }

        protected override int? GetSleepTime()
        {
            var response = Communication.GetResponse("/management/index.php?node=client&sub=configure");

            if (response.Error || response.IsFieldValid("#sleep")) return null;

            try
            {
                var sleepTime = int.Parse(response.GetField("#sleep"));
                if (sleepTime >= DefaultSleepTime)
                {
                    RegistryHandler.SetSystemSetting("Sleep", sleepTime.ToString());
                    return sleepTime;
                }

                Log.Entry(Name, string.Format("Sleep time set on the server is below the minimum of {0}", DefaultSleepTime));
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
