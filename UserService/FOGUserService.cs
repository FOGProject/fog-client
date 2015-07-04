using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using FOG.Handlers;
using FOG.Handlers.Data;
using FOG.Handlers.Power;
using FOG.Modules;
using FOG.Modules.AutoLogOut;
using FOG.Modules.DisplayManager;
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

            if (!data.action.Equals("update")) return;
            Power.SpawnUpdateWaiter(Assembly.GetExecutingAssembly().Location);
            Power.Updating = true;
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
                new DisplayManager()
            };
        }

        protected override void Load() { }
        protected override void Unload() { }

        protected override int? GetSleepTime()
        {
            try
            {
                var sleepTimeStr = RegistryHandler.GetSystemSetting("Sleep");
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

            var notificationProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\FOGNotificationGUI.exe",
                    Arguments = Transform.EncodeBase64(jsonData.ToString())
                }
            };
            notificationProcess.Start();
        }
    }
}
