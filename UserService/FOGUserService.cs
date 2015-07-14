using System;
using System.IO;
using System.Reflection;
using FOG.Handlers;
using FOG.Handlers.Data;
using FOG.Handlers.Power;
using FOG.Modules;
using FOG.Modules.AutoLogOut;
using FOG.Modules.DisplayManager;
using FOG.Modules.PrinterManager;
using Newtonsoft.Json;

namespace FOG
{
    class FOGUserService : AbstractService
    {
        public FOGUserService() : base()
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
            Log.Entry("User Service", "Spawning waiter");
            Power.SpawnUpdateWaiter(Assembly.GetExecutingAssembly().Location);
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
                new DisplayManager(),
                new DefaultPrinterManager()
            };
        }

        protected override void Load() { }
        protected override void Unload() { }

        protected override int? GetSleepTime()
        {
            try
            {
                var sleepTimeStr = Settings.Get("Sleep");
                var sleepTime = int.Parse(sleepTimeStr);
                if (sleepTime >= DefaultSleepTime)
                    return sleepTime;

                Log.Entry(Name, string.Format("Sleep time set on the server is below the minimum of {0}", DefaultSleepTime));
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

            ProcessHandler.RunEXE(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "FOGNotificationGUI.exe"),
                Transform.EncodeBase64(jsonData.ToString()), false);
        }
    }
}
