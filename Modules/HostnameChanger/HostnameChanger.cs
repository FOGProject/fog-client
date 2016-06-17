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
using Zazzles.Middleware;
using Zazzles.Modules;

namespace FOG.Modules.HostnameChanger
{
    /// <summary>
    ///     Rename a host, register with AD, and activate the windows key
    /// </summary>
    public class HostnameChanger : AbstractModule<HostnameChangerMessage>
    {
        private readonly IHostName _instance;

        public HostnameChanger()
        {
            Name = "HostnameChanger";

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

        protected override void DoWork(Response data, HostnameChangerMessage msg)
        {
            if (data.Error) return;
            if (!data.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            Log.Debug(Name, "AD Settings");
            Log.Debug(Name, "   Hostname:" + msg.Hostname);
            Log.Debug(Name, "   AD:" + msg.AD);
            Log.Debug(Name, "   ADDom:" + msg.ADDom);
            Log.Debug(Name, "   ADOU:" + msg.ADOU);
            Log.Debug(Name, "   ADUser:" + msg.ADUser);
            Log.Debug(Name, "   ADPW  :" + msg.ADPass);
            Log.Debug(Name, "   Enforce  :" + msg.Enforce);

            ActivateComputer(msg);

            if (!msg.Enforce && User.AnyLoggedIn())
            {
                Log.Entry(Name, "Users still logged in and enforce is disabled, delaying any further actions");
                return;
            }

            RenameComputer(msg);
            if (Power.ShuttingDown || Power.Requested) return;

            RegisterComputer(msg);
            if (Power.ShuttingDown || Power.Requested) return;
        }

        //Rename the computer and remove it from active directory
        private void RenameComputer(HostnameChangerMessage msg)
        {
            Log.Entry(Name, "Checking Hostname");
            if (string.IsNullOrEmpty(msg.Hostname))
            {
                Log.Error(Name, "Hostname is not specified");
                return;
            }
            if (Environment.MachineName.Equals(msg.Hostname.ToLower(), StringComparison.OrdinalIgnoreCase))
            {
                Log.Entry(Name, "Hostname is correct");
                return;
            }

            //First unjoin it from active directory
            UnRegisterComputer(msg);
            if (Power.ShuttingDown || Power.Requested) return;

            Log.Entry(Name, $"Renaming host to {msg.Hostname}");
            try
            {
                _instance.RenameComputer(msg.Hostname);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }

            Power.Restart(Settings.Get("Company") + " needs to rename your computer", Power.ShutdownOptions.Delay);
        }

        //Add a host to active directory
        private void RegisterComputer(HostnameChangerMessage msg)
        {
            if (!msg.AD)
                return;

            if (string.IsNullOrEmpty(msg.ADDom) || string.IsNullOrEmpty(msg.ADUser) ||
                string.IsNullOrEmpty(msg.ADPass))
            {
                Log.Error(Name, "Required ADDom Joining information is missing");
                return;
            }

            try
            {
                if (_instance.RegisterComputer(msg))
                    Power.Restart("Host joined to Active Directory, restart required", Power.ShutdownOptions.Delay);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        //Remove the host from active directory
        private void UnRegisterComputer(HostnameChangerMessage msg)
        {
            Log.Entry(Name, "Removing host from active directory");

            if (string.IsNullOrEmpty(msg.ADUser) || string.IsNullOrEmpty(msg.ADPass))
            {
                Log.Error(Name, "Required ADDom information is missing");
                return;
            }

            try
            {
                _instance.UnRegisterComputer(msg);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        //Active a computer with a product key
        private void ActivateComputer(HostnameChangerMessage msg)
        {
            if (string.IsNullOrEmpty(msg.Key))
                return;

            try
            {
                _instance.ActivateComputer(msg.Key);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }
    }
}
