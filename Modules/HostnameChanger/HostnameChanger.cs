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
            Description = "Rename a host, register with AD, and activate the windows key";

            _notifiedUser = false;
        }

        protected override void DoWork()
        {
            //Get task info
            var taskResponse = CommunicationHandler.GetResponse("/service/hostname.php?moduleid=" + Name.ToLower(), true);

            LogHandler.Debug(Name, "AD Settings");
            LogHandler.Debug(Name, "   Hostname:" + taskResponse.GetField("#hostname"));
            LogHandler.Debug(Name, "   AD:" + taskResponse.GetField("#AD"));
            LogHandler.Debug(Name, "   ADDom:" + taskResponse.GetField("#ADDom"));
            LogHandler.Debug(Name, "   ADOU:" + taskResponse.GetField("#ADOU"));
            LogHandler.Debug(Name, "   ADUser:" + taskResponse.GetField("#ADUser"));
            LogHandler.Debug(Name, "   ADPass:" + taskResponse.GetField("#ADPass"));

            if (taskResponse.Error) return;

            RenameComputer(taskResponse);
            if (!ShutdownHandler.ShutdownPending)
                RegisterComputer(taskResponse);
            if (!ShutdownHandler.ShutdownPending)
                ActivateComputer(taskResponse);
        }

        //Rename the computer and remove it from active directory
        private void RenameComputer(Response response)
        {
            LogHandler.Log(Name, "Checking Hostname");
            if (!response.IsFieldValid("#hostname"))
            {
                LogHandler.Error(Name, "Hostname is not specified");
                return;
            }
            if (string.Compare(Environment.MachineName, response.GetField("#hostname"), StringComparison.OrdinalIgnoreCase) == 1)
            {
                LogHandler.Error(Name, "Hostname is correct");
                return;
            }
            
            LogHandler.Log(Name, string.Format("Renaming host to {0}", response.GetField("#hostname")));

            if (!UserHandler.IsUserLoggedIn() || response.GetField("#force").Equals("1"))
            {
                LogHandler.Log(Name, "Unregistering computer");
                //First unjoin it from active directory
                UnRegisterComputer(response);

                LogHandler.Log(Name, "Updating registry");

                RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters","NV Hostname",
                    response.GetField("#hostname"));
                RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", "ComputerName",
                    response.GetField("#hostname"));
                RegistryHandler.SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName", "ComputerName",
                    response.GetField("#hostname"));

                ShutdownHandler.Restart(NotificationHandler.Company + " needs to rename your computer", 10);
            }
            else if(!_notifiedUser)
            {
                LogHandler.Log(Name, "User is currently logged in, will try again later");

                NotificationHandler.Notifications.Add(new Notification("Please log off",
                    string.Format("{0} is attemping to service your computer, please log off at the soonest available time", 
                    NotificationHandler.Company), 120));

                _notifiedUser = true;
            }
        }

        //Add a host to active directory
        private void RegisterComputer(Response response)
        {
            LogHandler.Log(Name, "Registering host with active directory");

            if (response.GetField("#AD") != "1")
            {
                LogHandler.Error(Name, "Active directory joining disabled for this host");
                return;
            }

            if (!response.IsFieldValid("ADDom") && !response.IsFieldValid("ADUser") && !response.IsFieldValid("ADPass"))
            {
                LogHandler.Error(Name, "Required Domain Joining information is missing");
                return;
            }

            // Attempt to join the domain
            var returnCode = DomainWrapper(response, true, (JoinOptions.NetsetupJoinDomain | JoinOptions.NetsetupAcctCreate));

            if (returnCode.Equals(2224))
                returnCode = DomainWrapper(response, true, JoinOptions.NetsetupJoinDomain);
            else if (returnCode.Equals(2))
                returnCode = DomainWrapper(response, false, (JoinOptions.NetsetupJoinDomain | JoinOptions.NetsetupAcctCreate));

            // Log the results
            LogHandler.Log(Name, string.Format("{0} {1}", (_returnCodes.ContainsKey(returnCode)
                ? string.Format("{0}, code = ", _returnCodes[returnCode])
                : "Unknown Return Code: "), returnCode));

            if (returnCode.Equals(0))
                ShutdownHandler.Restart("Host joined to Active Directory, restart required", 20);
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
            LogHandler.Log(Name, "Removing host from active directory");

            if (!response.IsFieldValid("#ADUser") || ! response.IsFieldValid("#ADPass"))
            {
                LogHandler.Error(Name, "Required Domain information is missing");
                return;
            }

            try
            {
                var returnCode = NetUnjoinDomain(null, response.GetField("#ADUser"), 
                    response.GetField("#ADPass"), UnJoinOptions.NetsetupAccountDelete);

                LogHandler.Log(Name, string.Format("{0} {1}", (_returnCodes.ContainsKey(returnCode)
                    ? string.Format("{0}, code = ", _returnCodes[returnCode])
                    : "Unknown Return Code: "), returnCode));

                if (returnCode.Equals(0))
                    ShutdownHandler.Restart("Host joined to active directory, restart needed", 20);
            }
            catch (Exception ex)
            {
                LogHandler.Error(Name, ex);
            }
        }

        //Active a computer with a product key
        private void ActivateComputer(Response response)
        {
            LogHandler.Log(Name, "Activing host with product key");

            if (!response.IsFieldValid("#Key"))
            {
                LogHandler.Error(Name, "Windows activation disabled");
                return;
            }
            if (response.GetField("#Key").Length != 29)
            {
                LogHandler.Error(Name, "Invalid product key");
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
                LogHandler.Error(Name, ex);
            }
        }
    }
}