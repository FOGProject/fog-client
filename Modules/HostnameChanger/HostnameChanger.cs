/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2016 FOG Project
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
using FOG.Modules.HostnameChanger.Linux;
using FOG.Modules.HostnameChanger.Mac;
using FOG.Modules.HostnameChanger.Windows;
using Zazzles;
using Zazzles.Modules;

namespace FOG.Modules.HostnameChanger
{
    /// <summary>
    ///     Rename a host, register with AD, and activate the windows key
    /// </summary>
    public sealed class HostnameChanger : PolicyModule<HostNameMessage>
    {
        private readonly IHostName _instance;
        public override string Name { get; protected set; }
        public override Settings.OSType Compatiblity { get; protected set; }

        public HostnameChanger()
        {
            Name = "HostnameChanger";
            Compatiblity = Settings.OSType.All;

            switch (Settings.OS)
            {
                case Settings.OSType.Mac:
                    _instance = new MacHostName();
                    break;
                case Settings.OSType.Linux:
                    _instance = new LinuxHostName();
                    break;
                default:
                    _instance = new WindowsHostName();
                    break;
            }
        }

        //Rename the computer and remove it from active directory
        private void RenameComputer(HostNameMessage data)
        {
            Log.Entry(Name, "Checking Hostname");

            if (Environment.MachineName.ToLower().Equals(data.HostName.ToLower()))
            {
                Log.Entry(Name, "Hostname is correct");
                return;
            }

            //First unjoin it from active directory
            UnRegisterComputer(data);
            if (Power.ShuttingDown || Power.Requested) return;

            Log.Entry(Name, $"Renaming host to {data.HostName}");

            try
            {
                _instance.RenameComputer(data.HostName.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }

            Power.Restart(Settings.Get("Company") + " needs to rename your computer", Power.ShutdownOptions.Delay);
        }

        //Add a host to active directory
        private void RegisterComputer(HostNameMessage data)
        {
            if (data.ActiveDirectory == false)
                return;

            try
            {
                if (_instance.RegisterComputer(data))
                    Power.Restart("Host joined to Active Directory, restart required", Power.ShutdownOptions.Delay);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        //Remove the host from active directory
        private void UnRegisterComputer(HostNameMessage data)
        {
            Log.Entry(Name, "Removing host from active directory");

            try
            {
                _instance.UnRegisterComputer(data);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        //Active a computer with a product key
        private void ActivateComputer(HostNameMessage data)
        {
            try
            {
                _instance.ActivateComputer(data.ProductKey);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        protected override void OnEvent(HostNameMessage message)
        {
            Log.Debug(Name, "AD Settings");
            Log.Debug(Name, "   Hostname:" + message.HostName);
            Log.Debug(Name, "   AD:" + message.ActiveDirectory);
            Log.Debug(Name, "   ADDom:" + message.Domain);
            Log.Debug(Name, "   ADOU:" + message.OU);
            Log.Debug(Name, "   ADUser:" + message.User);
            Log.Debug(Name, "   ADPW  :" + message.Password);

            RenameComputer(message);

            UnRegisterComputer(message);
            if (Power.ShuttingDown || Power.Requested) return;

            if (!Power.ShuttingDown && !Power.Requested)
                RegisterComputer(message);
            if (!Power.ShuttingDown && !Power.Requested)
                ActivateComputer(message);
        }
    }
}
