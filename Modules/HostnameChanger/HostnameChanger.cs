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
    public class HostnameChanger : AbstractModule
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

        protected override void DoWork()
        {
            //Get task info
            var taskResponse = Communication.GetResponse("/service/hostname.php?moduleid=" + Name.ToLower(), true);
            if (taskResponse.Error) return;
            if (!taskResponse.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            Log.Debug(Name, "AD Settings");
            Log.Debug(Name, "   Hostname:" + taskResponse.GetField("#hostname"));
            Log.Debug(Name, "   AD:" + taskResponse.GetField("#AD"));
            Log.Debug(Name, "   ADDom:" + taskResponse.GetField("#ADDom"));
            Log.Debug(Name, "   ADOU:" + taskResponse.GetField("#ADOU"));
            Log.Debug(Name, "   ADUser:" + taskResponse.GetField("#ADUser"));
            Log.Debug(Name, "   ADPW  :" + taskResponse.GetField("#ADPass"));

            RenameComputer(taskResponse);

            UnRegisterComputer(taskResponse);
            if (Power.ShuttingDown || Power.Requested) return;

            if (!Power.ShuttingDown && !Power.Requested)
                RegisterComputer(taskResponse);
            if (!Power.ShuttingDown && !Power.Requested)
                ActivateComputer(taskResponse);
        }

        //Rename the computer and remove it from active directory
        private void RenameComputer(Response response)
        {
            Log.Entry(Name, "Checking Hostname");
            if (!response.IsFieldValid("#hostname"))
            {
                Log.Error(Name, "Hostname is not specified");
                return;
            }
            if (Environment.MachineName.ToLower().Equals(response.GetField("#hostname").ToLower()))
            {
                Log.Entry(Name, "Hostname is correct");
                return;
            }

            //First unjoin it from active directory
            UnRegisterComputer(response);
            if (Power.ShuttingDown || Power.Requested) return;

            Log.Entry(Name, $"Renaming host to {response.GetField("#hostname")}");

            try
            {
                _instance.RenameComputer(response.GetField("#hostname"));
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }

            Power.Restart(Settings.Get("Company") + " needs to rename your computer", Power.ShutdownOptions.Delay);
        }

        //Add a host to active directory
        private void RegisterComputer(Response response)
        {
            if (response.GetField("#AD") != "1")
                return;

            if (!response.IsFieldValid("#ADDom") || !response.IsFieldValid("#ADUser") ||
                !response.IsFieldValid("#ADPass"))
            {
                Log.Error(Name, "Required Domain Joining information is missing");
                return;
            }

            try
            {
                if (_instance.RegisterComputer(response))
                    Power.Restart("Host joined to Active Directory, restart required", Power.ShutdownOptions.Delay);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        //Remove the host from active directory
        private void UnRegisterComputer(Response response)
        {
            Log.Entry(Name, "Removing host from active directory");

            if (!response.IsFieldValid("#ADUser") || !response.IsFieldValid("#ADPass"))
            {
                Log.Error(Name, "Required Domain information is missing");
                return;
            }

            try
            {
                _instance.UnRegisterComputer(response);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        //Active a computer with a product key
        private void ActivateComputer(Response response)
        {
            if (!response.IsFieldValid("#Key"))
                return;

            Log.Entry(Name, "Activing host with product key");

            try
            {
                _instance.ActivateComputer(response.GetField("#Key"));
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }
    }
}
