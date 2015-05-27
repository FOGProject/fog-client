using System;
using System.Linq;
using System.Threading;
using FOG.Handlers;
using FOG.Handlers.Middleware;
using FOG.Handlers.Power;
using FOG.Modules;

namespace FOG
{
    public abstract class AbstractService
    {
        // Basic variables every service needs
        public string Name { get; protected set; }
        private readonly AbstractModule[] _modules;
        protected const int DefaultSleepTime = 60;
        private readonly Thread _moduleThread;

        protected AbstractService()
        {
            _moduleThread = new Thread(ModuleLooper)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "FOGService"
            };

            _modules = GetModules();
            Name = "Service";
        }

        protected abstract AbstractModule[] GetModules();

        /// <summary>
        /// Start the service
        /// </summary>
        public virtual void Start()
        {
            // Only start if a valid server address is present
            if (string.IsNullOrEmpty(Configuration.ServerAddress)) return;
            _moduleThread.Start();
        }

        /// <summary>
        /// Loop through all the modules until an update or shutdown is pending
        /// </summary>
        protected virtual void ModuleLooper()
        {
            // Only run the service if there isn't a shutdown or update pending
            while (!Power.ShutdownPending && !Power.UpdatePending)
            {
                // Stop looping as soon as a shutdown or update pending
                foreach (var module in _modules.TakeWhile(module => !Power.ShutdownPending && !Power.UpdatePending))
                {
                    // Log file formatting
                    LogHandler.NewLine();
                    LogHandler.PaddedHeader(module.Name);
                    LogHandler.Log("Client-Info", string.Format("Version: {0}", RegistryHandler.GetSystemSetting("Version")));

                    try
                    {
                        module.Start();
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Error(Name, ex);
                    }

                    // Log file formatting
                    LogHandler.Divider();
                    LogHandler.NewLine();
                }


                // Skip checking for sleep time if there is a shutdown or update pending
                if (Power.ShutdownPending || Power.UpdatePending) break;

                // Once all modules have been run, sleep for the set time
                var sleepTime = GetSleepTime() ?? DefaultSleepTime;
                RegistryHandler.SetSystemSetting("Sleep", sleepTime.ToString());
                LogHandler.Log(Name, string.Format("Sleeping for {0} seconds", sleepTime));
                Thread.Sleep(sleepTime * 1000);
            } 
        }

        /// <summary>
        /// </summary>
        /// <returns>The number of seconds to sleep between running each module</returns>
        protected abstract int? GetSleepTime();

        /// <summary>
        /// Stop the service
        /// </summary>
        public virtual void Stop()
        {
            LogHandler.Log(Name, "Stop requested");
            _moduleThread.Abort();
        }

    }
}
