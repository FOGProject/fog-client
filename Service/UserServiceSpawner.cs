using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FOG.Handlers;

namespace FOG
{
    public static class UserServiceSpawner
    {
        private static readonly Dictionary<string, Process> Processes = new Dictionary<string, Process>();
        private static readonly Thread UserThread;
        private static volatile bool _running;

        static UserServiceSpawner()
        {
            UserThread = new Thread(UserChecker)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "FOGUserServiceSpawner"
            };
        }

        private static void UserChecker()
        {
            while (_running)
            {
                foreach (var user in UserHandler.GetUsersLoggedIn())
                {
                    if (Processes.ContainsKey(user)) continue;

                    var proc = ProcessHandler.RunClientEXEHandle("FOGUserService.exe", "");
                    Processes.Add(user, proc);
                }
            }
        }

        public static void Start()
        {
            _running = true;
            UserThread.Start();
        }

        public static void Stop()
        {
            _running = false;
        }

        public static void KillAll()
        {
            foreach(var proc in Processes.Values)
                proc.Kill();

            Processes.Clear();
        }
    }
}
