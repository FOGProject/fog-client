/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;


namespace FOG.Modules.PowerManagement
{
    /// <summary>
    ///     Perform cron style power tasks
    /// </summary>
    public class PowerManagement : AbstractModule<DataContracts.PowerManagement>
    {
        private ISchedulerFactory _schedulerFactory;
        private IScheduler _scheduler;

        private Dictionary<string, TriggerKey> _triggers;

        public PowerManagement()
        {
            Name = "PowerManagement";
            InitPowerManagement();
        }

        private async void InitPowerManagement()
        {
            _schedulerFactory = new StdSchedulerFactory();
            _scheduler = await _schedulerFactory.GetScheduler();
            await _scheduler.Start();

            _triggers = new Dictionary<string, TriggerKey>(StringComparer.OrdinalIgnoreCase);
        }

        protected override void DoWork(Response data, DataContracts.PowerManagement msg)
        {
            if (data.Error)
            {
                ClearAll();
                return;
            }

            if (!data.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                ClearAll();
                return;
            }

            if (msg.OnDemand.Equals("shutdown", StringComparison.OrdinalIgnoreCase))
            {
                Log.Entry(Name, "On demand shutdown requested");
                Power.Shutdown("FOG PowerManagement");
                return;
            }
            else if (msg.OnDemand.Equals("restart", StringComparison.OrdinalIgnoreCase))
            {
                Log.Entry(Name, "On demand restart requested");
                Power.Restart("FOG PowerManagement");
                return;
            }

            CreateTasks(msg.Tasks);
        }

        private IJobDetail JobFactory(bool restart)
        {
            var group = (restart) ? "restart" : "shutdown";
            var name = Guid.NewGuid().ToString();

            if (restart)
            {
                return JobBuilder.Create <RestartJob> ()
                    .WithIdentity(name, group)
                    .Build();
            }
            else
            {
                return JobBuilder.Create<ShutdownJob>()
                     .WithIdentity(name, group)
                     .Build();
            }
        }

        private void ClearAll()
        {
            foreach (var triggerPair in _triggers)
            {
                Log.Debug(Name, $"--> Unscheduling a {triggerPair.Key}");
                _scheduler.UnscheduleJob(triggerPair.Value);
            }

            _triggers.Clear();
        }

        private void RemoveExtraTasks(List<Task> tasks)
        {
            Log.Entry(Name, "Calculating tasks to unschedule");
            var toRemove = new Dictionary<string, TriggerKey>(_triggers);

            foreach (var task in tasks)
            {
                toRemove.Remove(task.ToString());
            }

            foreach (var triggerPair in toRemove)
            {
                Log.Debug(Name, $"--> Unscheduling a {triggerPair.Key}");
                try
                {
                    _scheduler.UnscheduleJob(triggerPair.Value);
                    _triggers.Remove(triggerPair.Key);
                }
                catch (Exception ex)
                {
                    Log.Error(Name, ex);
                }

            }
        }

        private void CreateTasks(List<Task> tasks)
        {
            RemoveExtraTasks(tasks);

            Log.Entry(Name, "Calculating tasks to schedule");
            foreach (var task in tasks)
            {
                if (string.IsNullOrWhiteSpace(task.Action) || string.IsNullOrWhiteSpace(task.CRON))
                {
                    Log.Debug(Name, "--> Invalid task given by server: " + task);
                    continue;
                }

                if (_triggers.ContainsKey(task.ToString()))
                {
                    Log.Debug(Name, $"--> A {task} is already scheduled");
                    continue;
                }

                Log.Debug(Name, $"--> Scheduling a {task} with a Quartz of {task.ToQuartz()}");

                try
                {
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(task.CRON, task.Action)
                        .WithCronSchedule(task.ToQuartz(), x => x.WithMisfireHandlingInstructionDoNothing())
                        .Build();
                    var key = trigger.Key;
                    _triggers.Add(task.ToString(), key);

                    var isRestart = task.Action.Equals("reboot", StringComparison.OrdinalIgnoreCase);

                    _scheduler.ScheduleJob(JobFactory(isRestart), trigger);
                }
                catch (Exception ex)
                {
                    Log.Error(Name, ex);
                }

            }
        }
    }

    internal class ShutdownJob : IJob
    {
        async System.Threading.Tasks.Task IJob.Execute(IJobExecutionContext context)
        {
            await Power.Shutdown("FOG PowerManagement");
        }
    }

    internal class RestartJob : IJob
    {
        async System.Threading.Tasks.Task IJob.Execute(IJobExecutionContext context)
        {
            await Power.Restart("FOG PowerManagement");
        }
    }

}