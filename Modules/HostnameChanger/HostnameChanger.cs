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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        private bool _notifiedUser;

        private readonly Dictionary<int, string> _returnCodes = new Dictionary<int, string>
        {
            {0, "Success"},
            {2, "The OU parameter is not set properly or not working with this current setup"},
            {5, "Access Denied"},
            {87, "The parameter is incorrect"},
            {110, "The system cannot open the specified object"},
            {1326, "Logon failure: unknown username or bad password"},
            {1355, "The specified domain either does not exist or could not be contacted"},
            {2103, "The server could not be located"},
            {2105, "A network resource shortage occured"},
            {2691, "The machine is already joined to the domain"},
            {2692, "The machine is not currently joined to a domain"}
        };
            
        [Flags]
        private enum UnJoinOptions
        {
            NetsetupAccountDelete = 0x00000004
        }

        [Flags]
        private enum JoinOptions
        {
            NetsetupJoinDomain = 0x00000001,
            NetsetupAcctCreate = 0x00000002,
        }

        //Import dll methods
        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetJoinDomain(string lpServer, string lpDomain, string lpAccountOU,
            string lpAccount, string lpPassword, JoinOptions nameType);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetUnjoinDomain(string lpServer, string lpAccount, string lpPassword,
            UnJoinOptions fUnjoinOptions);

        public HostnameChanger()
        {
            Name = "HostnameChanger";
            _notifiedUser = false;
        }

        protected override void DoWork()
        {
            //Get task info
            var taskResponse = Communication.GetResponse("/service/hostname.php?moduleid=" + Name.ToLower(), true);

            Log.Debug(Name, "AD Settings");
            Log.Debug(Name, "   Hostname:" + taskResponse.GetField("#hostname"));
            Log.Debug(Name, "   AD:" + taskResponse.GetField("#AD"));
            Log.Debug(Name, "   ADDom:" + taskResponse.GetField("#ADDom"));
            Log.Debug(Name, "   ADOU:" + taskResponse.GetField("#ADOU"));
            Log.Debug(Name, "   ADUser:" + taskResponse.GetField("#ADUser"));
            Log.Debug(Name, "   ADPass:" + taskResponse.GetField("#ADPass"));

            if (taskResponse.Error) return;

            RenameComputer(taskResponse);
            if (!Power.ShutdownPending)
                RegisterComputer(taskResponse);
            if (!Power.ShutdownPending)
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
            
            Log.Entry(Name, string.Format("Renaming host to {0}", response.GetField("#hostname")));

            if (!UserHandler.IsUserLoggedIn() || response.GetField("#force").Equals("1"))
            {
                Log.Entry(Name, "Unregistering computer");
                //First unjoin it from active directory
                UnRegisterComputer(response);

                Log.Entry(Name, "Updating registry");

                RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters","NV Hostname",
                    response.GetField("#hostname"));
                RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", "ComputerName",
                    response.GetField("#hostname"));
                RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName", "ComputerName",
                    response.GetField("#hostname"));

                Power.Restart(NotificationHandler.Company + " needs to rename your computer", 10);
            }
            else if(!_notifiedUser)
            {
                Log.Entry(Name, "User is currently logged in, will try again later");

                NotificationHandler.Notifications.Add(new Notification("Please log off",
                    string.Format("{0} is attemping to service your computer, please log off at the soonest available time", 
                    NotificationHandler.Company), 120));

                _notifiedUser = true;
            }
        }

        //Add a host to active directory
        private void RegisterComputer(Response response)
        {
            Log.Entry(Name, "Registering host with active directory");

            if (response.GetField("#AD") != "1")
            {
                Log.Error(Name, "Active directory joining disabled for this host");
                return;
            }

            if (!response.IsFieldValid("#ADDom") || !response.IsFieldValid("#ADUser") || !response.IsFieldValid("#ADPass"))
            {
                Log.Error(Name, "Required Domain Joining information is missing");
                return;
            }

            // Attempt to join the domain
            var returnCode = DomainWrapper(response, true, (JoinOptions.NetsetupJoinDomain | JoinOptions.NetsetupAcctCreate));

            if (returnCode.Equals(2224))
                returnCode = DomainWrapper(response, true, JoinOptions.NetsetupJoinDomain);
            else if (returnCode.Equals(2))
                returnCode = DomainWrapper(response, false, (JoinOptions.NetsetupJoinDomain | JoinOptions.NetsetupAcctCreate));

            // Entry the results
            Log.Entry(Name, string.Format("{0} {1}", (_returnCodes.ContainsKey(returnCode)
                ? string.Format("{0}, code = ", _returnCodes[returnCode])
                : "Unknown Return Code: "), returnCode));

            if (returnCode.Equals(0))
                Power.Restart("Host joined to Active Directory, restart required", 20);
        }

        private static int DomainWrapper(Response response, bool ou, JoinOptions options)
        {
            return NetJoinDomain(null,
                response.GetField("#ADDom"),
                ou ? response.GetField("#ADOU") :  null,
                response.GetField("#ADUser"),
                response.GetField("#ADPass"),
                options);           
        }

        //Remove the host from active directory
        private void UnRegisterComputer(Response response)
        {
            Log.Entry(Name, "Removing host from active directory");

            if (!response.IsFieldValid("#ADUser") || ! response.IsFieldValid("#ADPass"))
            {
                Log.Error(Name, "Required Domain information is missing");
                return;
            }

            try
            {
                var returnCode = NetUnjoinDomain(null, response.GetField("#ADUser"), 
                    response.GetField("#ADPass"), UnJoinOptions.NetsetupAccountDelete);

                Log.Entry(Name, string.Format("{0} {1}", (_returnCodes.ContainsKey(returnCode)
                    ? string.Format("{0}, code = ", _returnCodes[returnCode])
                    : "Unknown Return Code: "), returnCode));

                if (returnCode.Equals(0))
                    Power.Restart("Host joined to active directory, restart needed", 20);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        //Active a computer with a product key
        private void ActivateComputer(Response response)
        {
            Log.Entry(Name, "Activing host with product key");

            if (!response.IsFieldValid("#Key"))
            {
                Log.Error(Name, "Windows activation disabled");
                return;
            }
            if (response.GetField("#Key").Length != 29)
            {
                Log.Error(Name, "Invalid product key");
                return;
            }

            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = @"cscript",
                        Arguments = string.Format("//B //Nologo {0}\\slmgr.vbs /ipk {1}", 
                            Environment.SystemDirectory, response.GetField("#Key")),
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                //Give windows the new key
                process.Start();
                process.WaitForExit();
                process.Close();

                //Try and activate the new key
                process.StartInfo.Arguments = string.Format("//B //Nologo {0}\\slmgr.vbs /ato", Environment.SystemDirectory);
                process.Start();
                process.WaitForExit();
                process.Close();

            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }
    }
}