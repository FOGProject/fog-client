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

using FOG.Handlers;

namespace FOG.Modules
{
    /// <summary>
    ///     The base of all FOG Modules
    /// </summary>
    public abstract class AbstractModule
    {
        protected AbstractModule()
        {
            Name = "Generic Module";
            Description = "Generic Description";
            EnabledURL = "/service/servicemodule-active.php";
        }

        //Basic variables every module needs
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public string EnabledURL { get; protected set; }

        /// <summary>
        ///     Called to Start the module. Filters out modules that are disabled on the server
        /// </summary>
        public virtual void Start()
        {
            LogHandler.Log(Name, "Running...");
            if (isEnabled())
            {
                doWork();
            }
        }

        /// <summary>
        ///     Called after Start() filters out disabled modules. Contains the module's functionality
        /// </summary>
        protected abstract void doWork();

        /// <summary>
        ///     Check if the module is enabled
        /// </summary>
        /// <returns>True if the module is enabled</returns>
        public bool isEnabled()
        {
            var moduleActiveResponse = CommunicationHandler.GetResponse(string.Format("{0}?moduleid={1}", EnabledURL, Name.ToLower()), true);
            return !moduleActiveResponse.Error;
        }
    }
}