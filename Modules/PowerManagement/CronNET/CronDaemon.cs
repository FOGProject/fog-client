using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading;

namespace FOG.Modules.PowerManagement.CronNET
{
    public class CronDaemon
    {
        private readonly System.Timers.Timer _timer = new System.Timers.Timer(30000);
        private readonly List<CronJob> _cronJobs = new List<CronJob>();
        private DateTime _last= DateTime.Now;

        public CronDaemon()
        {
            _timer.AutoReset = true;
            _timer.Elapsed += timer_elapsed;
        }

        public void AddJob(string schedule, ThreadStart action)
        {
            var cj = new CronJob(schedule, action);
            _cronJobs.Add(cj);
        }

        public void RemoveJob(string schedule, ThreadStart action)
        {
            CronJob toRemove = null;

            foreach (var job in _cronJobs.Where(job => job.CronSchedule.ToString().Equals(schedule) && action == job.ThreadStart))
            {
                toRemove = job;
                break;
            }

            if (toRemove == null)
                return;

            _cronJobs.Remove(toRemove);

        }

        public void RemoveAllJobs()
        {
            foreach (var job in _cronJobs)
                job.Abort();

            _cronJobs.Clear();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            RemoveAllJobs();
        }

        private void timer_elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Minute == _last.Minute) return;
            _last = DateTime.Now;
            foreach (var job in _cronJobs)
                job.Execute(DateTime.Now);
        }
    }
}
