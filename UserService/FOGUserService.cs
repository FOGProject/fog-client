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

        private static PipeClient _servicePipe;

        public FOGUserService() : base()
        {
            //Setup the service pipe client
            _servicePipe = new PipeClient("fog_pipe_service");
            _servicePipe.MessageReceived += pipeClient_MessageReceived;
        }

        //Handle recieving a message
        private void pipeClient_MessageReceived(string message)
        {
            Log.Debug(Name, "Message recieved from service");
            Log.Debug(Name, string.Format("MSG: {0}", message));

            if (!message.Equals("UPD")) return;

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

        protected override void Load()
        {
            _servicePipe.Connect();
        }

        protected override void Unload()
        {
            _servicePipe.Kill();
        }

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
