using System;
using System.Threading;

namespace FOG.Modules.PowerManagement.CronNET
{
    public class CronJob
    {
        public readonly CronSchedule CronSchedule = new CronSchedule();
        public readonly ThreadStart ThreadStart;
        private readonly object _lock = new object();
        private Thread _thread;

        public CronJob(string schedule, ThreadStart thread_start)
        {
            CronSchedule = new CronSchedule(schedule);
            ThreadStart = thread_start;
            _thread = new Thread(thread_start);
        }

        public void Execute(DateTime date_time)
        {
            lock (_lock)
            {
                if (!CronSchedule.IsTime(date_time))
                    return;

                if (_thread.ThreadState == ThreadState.Running)
                    return;

                _thread = new Thread(ThreadStart);
                _thread.Start();
            }
        }

        public void Abort()
        {
            _thread.Abort();  
        }
    }
}
