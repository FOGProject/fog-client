using System;
using System.Reflection;
using FOG.Handlers;
using FOG.Handlers.Power;
using FOG.Modules;
using FOG.Modules.AutoLogOut;
using FOG.Modules.DisplayManager;

namespace FOG
{
    class FOGUserService : AbstractService
    {
        public FOGUserService() : base()
        {
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Update, OnUpdate);
        }

        //Handle recieving a message
        private static void OnUpdate(dynamic data)
        {
            if (!data.action.Equals("update")) return;
            Power.SpawnUpdateWaiter(Assembly.GetExecutingAssembly().Location);
            Power.Updating = true;
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
    }
}
