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
using FOG.Handlers;
using FOG.Handlers.Middleware;
using FOG.Handlers.Power;

namespace FOG.Modules.HostnameChanger
{
    /// <summary>
    ///     Rename a host, register with AD, and activate the windows key
    /// </summary>
    public class HostnameChanger : AbstractModule
    {
        public HostnameChanger()
        {
            Name = "HostnameChanger";
        }

        protected override void DoWork()
        {
            //Get task info
            var taskResponse = Communication.GetResponse("/service/hostname.php?moduleid=" + Name.ToLower(), true);

            Log.Debug(Name, "AD Settings");
            Log.Debug(Name, "   Hostname:  " + taskResponse.GetField("#hostname"));
            Log.Debug(Name, "   AD:        " + taskResponse.GetField("#AD"));
            Log.Debug(Name, "   ADDom:     " + taskResponse.GetField("#ADDom"));
            Log.Debug(Name, "   ADOU:      " + taskResponse.GetField("#ADOU"));
            Log.Debug(Name, "   ADUser:    " + taskResponse.GetField("#ADUser"));
            Log.Debug(Name, "   Enforce:   " + taskResponse.GetField("#enforce"));

            if (taskResponse.Error) return;

            if (!taskResponse.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            if (UserHandler.IsUserLoggedIn() && !taskResponse.GetField("#enforce").Equals("1"))
            {
                Log.Entry(Name, "Enforcement is disabled and a user is logged in, skipping module");
            }

            RenameComputer(taskResponse);

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
            
            Log.Entry(Name, $"Renaming host to {response.GetField("#hostname")}");

            UnRegisterComputer(response);

            if (Power.ShuttingDown || Power.Requested) return;
            UpdateHostnameRegistry(response.GetField("#hostname"));

            Power.Restart(RegistryHandler.GetSystemSetting("Company") + " needs to rename your computer", Power.FormOption.Delay);
        }

        private void UpdateHostnameRegistry(string hostname)
        {
            Log.Entry(Name, "Updating registry, '");
            RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "NV Hostname", hostname);
            RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", "ComputerName", hostname);
            RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName", "ComputerName", hostname);
        }

        //Add a host to active directory
        private void RegisterComputer(Response response)
        {
            if (response.GetField("#AD") != "1")
                return;

            Log.Entry(Name, "Registering host with active directory");

            if (!response.AreFieldsValid("#ADDom", "#ADUser", "#ADPass"))
            {
                Log.Error(Name, "Required Domain Joining information is missing");
                return;
            }

            if (WinDomain.InCorrectDomain(response))
            {
                Log.Entry(Name, "Host is already joined to target domain");
                return;
            }

            var returnCode = WinDomain.Join(response);
            Log.Entry(Name, "Domain Join return code = " + returnCode);

            if(WinDomain.ReturnCodes.ContainsKey(returnCode))
                Log.Entry(Name, WinDomain.ReturnCodes[returnCode]);

            if (returnCode.Equals(0))
                Power.Restart("Host joined to Active Directory, restart required", Power.FormOption.Delay);
        }

        //Remove the host from active directory
        private void UnRegisterComputer(Response response)
        {
            Log.Entry(Name, "Removing host from active directory");

            if (!response.AreFieldsValid("#ADUser", "#ADPass"))
            {
                Log.Error(Name, "Required Domain information is missing");
                return;
            }

            var returnCode = WinDomain.Leave(response);
            Log.Entry(Name, "Domain Leave return code = " + returnCode);

            if (WinDomain.ReturnCodes.ContainsKey(returnCode))
                Log.Entry(Name, WinDomain.ReturnCodes[returnCode]);

            if (returnCode.Equals(0))
                Power.Restart("Host left active directory, restart needed", Power.FormOption.Delay);
        }

        //Active a computer with a product key
        private void ActivateComputer(Response response)
        {
            if (!response.IsFieldValid("#Key"))
                return;

            Log.Entry(Name, "Checking Product Key Activation");
            var key = response.GetField("#Key");
            if (key.Length != 29)
            {
                Log.Error(Name, "Invalid product key provided by server");
                return;
            }

            var partialKey = WinActivation.GetPartialKey();

            if (key.EndsWith(partialKey))
            {
                if (!WinActivation.IsActivated())
                {
                    Log.Entry(Name, "Windows has correct key but is not licensed");
                }
                else
                {
                    Log.Entry(Name, "Already activated with correct key");
                    return;
                }
            }

            WinActivation.SetProductKey(key);
        }
    }
}