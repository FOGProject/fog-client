/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2015 FOG Project
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
        protected const int DefaultSleepTime = 60;
        private readonly AbstractModule[] _modules;
        private readonly Thread _moduleThread;

        protected AbstractService()
        {
            _moduleThread = new Thread(ModuleLooper)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = false,
                Name = "FOGService"
            };

            _modules = GetModules();
            Name = "Service";
        }

        // Basic variables every service needs
        public string Name { get; protected set; }
        protected abstract AbstractModule[] GetModules();
        protected abstract void Load();
        protected abstract void Unload();

        /// <summary>
        ///     Start the service
        /// </summary>
        public virtual void Start()
        {
            // Only start if a valid server address is present
            if (string.IsNullOrEmpty(Configuration.ServerAddress)) return;
            Load();
            _moduleThread.Start();
        }

        /// <summary>
        ///     Loop through all the modules until an update or shutdown is pending
        /// </summary>
        protected virtual void ModuleLooper()
        {
            // Only run the service if there isn't a shutdown or update pending
            while (!Power.ShuttingDown && !Power.Updating)
            {
                // Stop looping as soon as a shutdown or update pending
                foreach (var module in _modules.TakeWhile(module => !Power.ShuttingDown && !Power.Updating))
                {
                    // Entry file formatting
                    Log.NewLine();
                    Log.PaddedHeader(module.Name);
                    Log.Entry("Client-Info", string.Format("Version: {0}", Settings.Get("Version")));

                    try
                    {
                        module.Start();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(Name, ex);
                    }

                    // Entry file formatting
                    Log.Divider();
                    Log.NewLine();

                    if (Power.Requested)
                        break;
                }

                while (Power.Requested)
                {
                    Log.Entry(Name, "Power operation being requested, checking back in 30 seconds");
                    Thread.Sleep(30*1000);
                }

                // Skip checking for sleep time if there is a shutdown or update pending
                if (Power.ShuttingDown || Power.Updating) break;

                // Once all modules have been run, sleep for the set time
                var sleepTime = GetSleepTime() ?? DefaultSleepTime;
                Log.Entry(Name, string.Format("Sleeping for {0} seconds", sleepTime));
                Thread.Sleep(sleepTime*1000);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>The number of seconds to sleep between running each module</returns>
        protected abstract int? GetSleepTime();

        /// <summary>
        ///     Stop the service
        /// </summary>
        public virtual void Stop()
        {
            Log.Entry(Name, "Stop requested");
            _moduleThread.Abort();
            Unload();
        }
    }
}