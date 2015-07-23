using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                var users = UserHandler.GetUsersLoggedIn();

                foreach (var user in users)
                {
                    if (Processes.ContainsKey(user)) continue;

                    var proc = ProcessHandler.ImpersonateClientEXEHandle("FOGUserService.exe", "", user);
                    Processes.Add(user, proc);
                }

                var loggedOff = users.Except(Processes.Keys);
                foreach (var user in loggedOff)
                {
                    Processes[user].Kill();
                    Processes[user].Dispose();
                    Processes.Remove(user);
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
            foreach (var proc in Processes.Values)
            {
                proc.Kill();
                proc.Dispose();
            }

            Processes.Clear();
        }
    }
}
