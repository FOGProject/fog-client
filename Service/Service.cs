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

using System.ServiceProcess;
using Zazzles;

namespace FOG
{
    /// <summary>
    ///     Coordinate all system wide FOG modules
    /// </summary>
    public class Service : ServiceBase
    {
        private readonly AbstractService _fogService;

        public Service()
        {
            Log.Entry("Controller", "Initialize");
            //Initialize everything
            _fogService = new FOGSystemService();
        }

        //Called when the service starts
        protected override void OnStart(string[] args)
        {
            Log.Entry("Controller", "Start");
            _fogService.Start();
        }

        //Called when the service stops
        protected override void OnStop()
        {
            Log.Entry("Controller", "Stop");
            _fogService.Stop();
        }
    }
}