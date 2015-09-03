using System;
using System.Diagnostics;
using FOG.Handlers;
using FOG.Handlers.Middleware;
using FOG.Handlers.Power;
using FOG.Modules;
using FOG.Modules.ClientUpdater;
using FOG.Modules.GreenFOG;
using FOG.Modules.HostnameChanger;
using FOG.Modules.PrinterManager;
using FOG.Modules.SnapinClient;
using FOG.Modules.TaskReboot;
using FOG.Modules.UserTracker;
using Newtonsoft.Json.Linq;

namespace FOG
{
    public class FOGSystemService : AbstractService
    {
        public FOGSystemService() : base()
        {
            Bus.SetMode(Bus.Mode.Server);
        }

        protected override void Load()
        {
            // Kill the sub-processes
            foreach (var process in Process.GetProcessesByName("FOGUserService"))
                process.Kill();

            foreach (var process in Process.GetProcessesByName("FOGTray"))
                process.Kill();

            dynamic json = new JObject();
            json.action = "load";
            Bus.Emit(Bus.Channel.Status, json, true);

            Log.NewLine();
            Log.PaddedHeader("Authentication");
            Log.Entry("Client-Info", string.Format("Version: {0}", RegistryHandler.GetSystemSetting("Version")));
            if (!Authentication.HandShake()) return;
            Log.NewLine();
        }


        protected override void Unload()
        {
            dynamic json = new JObject();
            json.action = "unload";
            Bus.Emit(Bus.Channel.Status, json, true); Bus.Dispose();

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
                new PrinterManager(), 
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

            if (response.Error) return null;

            // Set the shutdown graceperiod
            RegistryHandler.SetSystemSetting("gracePeriod", response.GetField("#promptTime"));

            if (!response.IsFieldValid("#sleep")) return null;

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
