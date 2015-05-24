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
            _servicePipe.Connect();
        }

        public override void Start()
        {
            _servicePipe.Connect();
            base.Start();
        }

        //Handle recieving a message
        private void pipeClient_MessageReceived(string message)
        {
            LogHandler.Debug(Name, "Message recieved from service");
            LogHandler.Debug(Name, string.Format("MSG: {0}", message));

            if (!message.Equals("UPD")) return;

            Power.SpawnUpdateWaiter(Assembly.GetExecutingAssembly().Location);
            Power.UpdatePending = true;
        }

        public override void Stop()
        {
            _servicePipe.Kill();
            base.Stop();
        }

        protected override AbstractModule[] GetModules()
        {
            return new AbstractModule[]
            {
                new AutoLogOut(), 
                new DisplayManager()
            };
        }

        protected override int? GetSleepTime()
        {
            var sleepTimeStr = RegistryHandler.GetSystemSetting("Sleep");
            if (string.IsNullOrEmpty(sleepTimeStr)) return null;

            try
            {
                var sleepTime = int.Parse(sleepTimeStr);
                if (sleepTime >= DefaultSleepTime)
                    return sleepTime;

                LogHandler.Log(Name, string.Format("Sleep time set on the server is below the minimum of {0}", DefaultSleepTime));
            }
            catch (Exception ex)
            {
                LogHandler.Error(Name, "Unable to parse sleep time");
                LogHandler.Error(Name, ex);
            }

            return null;
        }
    }
}
