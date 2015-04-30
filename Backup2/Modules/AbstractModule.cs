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
using FOG.Handlers;

namespace FOG.Modules
{
    /// <summary>
    /// The base of all FOG Modules
    /// </summary>
    public abstract class AbstractModule
    {

        //Basic variables every module needs
        public String Name { get; protected set; }
        public String Description { get; protected set; }
        public String EnabledURL { get; protected set; }

        protected AbstractModule()
        {
            Name = "Generic Module";
            Description = "Generic Description";
            EnabledURL = "/service/servicemodule-active.php";
        }

        /// <summary>
        /// Called to start the module. Filters out modules that are disabled on the server
        /// </summary>
        public virtual void start()
        {
            LogHandler.Log(Name, "Running...");
            if (isEnabled())
            {
                doWork();
            }
        }

        /// <summary>
        /// Called after start() filters out disabled modules. Contains the module's functionality
        /// </summary>
        protected abstract void doWork();

        /// <summary>
        /// Check if the module is enabled
        /// </summary>
        /// <returns>True if the module is enabled</returns>
        public Boolean isEnabled()
        {
            var moduleActiveResponse = CommunicationHandler.GetResponse(EnabledURL + "?moduleid=" + Name.ToLower(), true);
            return !moduleActiveResponse.Error;
        }

    }
}