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

using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Zazzles;

namespace FOG.Modules.HostnameChanger.Windows
{
    internal class WindowsHostName : IHostName
    {
        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetJoinDomain(string lpServer, string lpDomain, string lpAccountOU, 
            string lpAccount, string lpPassword, JoinOptions nameType);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetUnjoinDomain(string lpServer, string lpAccount, string lpPassword,
            UnJoinOptions fUnjoinOptions);

        [DllImport("netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int NetRenameMachineInDomain(string lpServer, string lpNewMachineName,
            string lpAccount, string lpPassword, JoinOptions fRenameOptions);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetComputerNameEx(COMPUTER_NAME_FORMAT NameType, string lpBuffer);


        private static int DomainWrapper(DataContracts.HostnameChanger msg, bool ou, JoinOptions options)
        {
            return NetJoinDomain(null,
                msg.ADDom,
                ou ? msg.ADOU : null,
                msg.ADUser,
                msg.ADPass,
                options);
        }

        [Flags]
        private enum COMPUTER_NAME_FORMAT
        {
            ComputerNameNetBIOS,
            ComputerNameDnsHostname,
            ComputerNameDnsDomain,
            ComputerNameDnsFullyQualified,
            ComputerNamePhysicalNetBIOS,
            ComputerNamePhysicalDnsHostname,
            ComputerNamePhysicalDnsDomain,
            ComputerNamePhysicalDnsFullyQualified,
        }

        [Flags]
        private enum UnJoinOptions
        {
            NetsetupAccountDelete = 0x00000004
        }

        [Flags]
        private enum JoinOptions
        {
            NETSETUP_JOIN_DOMAIN = 0x00000001,
            NETSETUP_DOMAIN_JOIN_IF_JOINED = 0x00000020,
            NETSETUP_JOIN_WITH_NEW_NAME = 0x00000400,
            NETSETUP_ACCT_CREATE = 0x00000002
        }

        private readonly Dictionary<int, string> _returnCodes = new Dictionary<int, string>
        {
            {0, "Success"},
            {2, "The ADOU parameter is not set properly or not working with this current setup"},
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

        private readonly string Name = "HostnameChanger";

        public bool RenameComputer(DataContracts.HostnameChanger msg)
        {
            var success = false;
            if (IsJoinedToDomain(msg.ADDom))
            {
                if (!msg.AD)
                {
                    Log.Error(Name, "Host is currently bound to a domain, but domain management is disabled; Cannot rename");
                    return false;
                }

                if (string.IsNullOrEmpty(msg.ADDom) || string.IsNullOrEmpty(msg.ADUser) ||
                    string.IsNullOrEmpty(msg.ADPass))
                {
                    Log.Error(Name, "Host is currently bound to a domain, but required domain information is missing; Cannot rename");
                    return false;
                }

                Log.Entry(Name, "Renaming host inside existing domain binding");
                // We are already in the correct domain
                var returnCode = NetRenameMachineInDomain(null, msg.Hostname, msg.ADUser, msg.ADPass, JoinOptions.NETSETUP_ACCT_CREATE);
                Log.Entry(Name,
                $"{(_returnCodes.ContainsKey(returnCode) ? $"{_returnCodes[returnCode]}, code = " : "Unknown Return Code: ")} {returnCode}");

                if (returnCode == 0)
                {
                    success = true;
                    SetLocalHostName(msg.Hostname);
                }
            } else if (IsJoinedToAnyDomain()) {
                if (!msg.AD)
                {
                    Log.Error(Name, "Host is currently bound to a domain, but domain management is disabled; Cannot rename");
                    return false;
                }

                if (string.IsNullOrEmpty(msg.ADDom) || string.IsNullOrEmpty(msg.ADUser) ||
                    string.IsNullOrEmpty(msg.ADPass))
                {
                    Log.Error(Name, "Host is currently bound to a domain, but required domain information is missing; Cannot rename");
                    return false;
                }
                Log.Entry(Name, "Moving host to correct domain");
                // We are in the incorrect domain
                success = UnRegisterComputer(msg);
            } else
            {
                // We are not joined to any domain
                success = SetLocalHostName(msg.Hostname);


                if (success && msg.AD && !string.IsNullOrEmpty(msg.ADDom) && !string.IsNullOrEmpty(msg.ADUser) && !string.IsNullOrEmpty(msg.ADPass))
                {
                    Log.Entry(Name, "Joining domain");
                    var returnCode =  DomainWrapper(msg, true, JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_JOIN_WITH_NEW_NAME | JoinOptions.NETSETUP_ACCT_CREATE);
                    switch (returnCode)
                    {
                        case 2224:
                            returnCode = DomainWrapper(msg, true, JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_JOIN_WITH_NEW_NAME);
                            break;
                        case 2:
                        case 50:
                        case 1355:
                            returnCode = DomainWrapper(msg, false,
                                (JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_JOIN_WITH_NEW_NAME | JoinOptions.NETSETUP_ACCT_CREATE));
                            break;
                    }
                    Log.Entry(Name,
                    $"{(_returnCodes.ContainsKey(returnCode) ? $"{_returnCodes[returnCode]}, code = " : "Unknown Return Code: ")} {returnCode}");
                } 
            }

            if (success)
                Power.Restart(Settings.Get("Company") + " needs to rename your computer", Power.ShutdownOptions.Delay);

            return success;
        }

        private bool SetLocalHostName(string hostname)
        {
            return SetComputerNameEx(COMPUTER_NAME_FORMAT.ComputerNamePhysicalDnsHostname, hostname);
        }


        private bool IsJoinedToAnyDomain()
        {
            try
            {
                using (var domain = Domain.GetComputerDomain())
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        private bool IsJoinedToDomain(string idealDomain)
        {
            try
            {
                using (var domain = Domain.GetComputerDomain())
                {
                    var currentIP = Dns.GetHostAddresses(domain.Name);
                    var targetIP = Dns.GetHostAddresses(idealDomain);

                    return (currentIP.Intersect(targetIP).Any());
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public bool RegisterComputer(DataContracts.HostnameChanger msg)
        {
            // Check if the host is already part of the set domain by checking server IPs
            if (IsJoinedToDomain(msg.ADDom))
            {
                Log.Entry(Name, "Host already joined to target domain");
                return true;
            }

            // Attempt to join the domain
            var returnCode = DomainWrapper(msg, true,
                (JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_ACCT_CREATE));

            switch (returnCode)
            {
                case 2224:
                    returnCode = DomainWrapper(msg, true, JoinOptions.NETSETUP_JOIN_DOMAIN);
                    break;
                case 2:
                case 50:
                case 1355:
                    returnCode = DomainWrapper(msg, false,
                        (JoinOptions.NETSETUP_JOIN_DOMAIN | JoinOptions.NETSETUP_ACCT_CREATE));
                    break;
            }

            // Entry the results
            Log.Entry(Name,
                $"{(_returnCodes.ContainsKey(returnCode) ? $"{_returnCodes[returnCode]}, code = " : "Unknown Return Code: ")} {returnCode}");

            if (returnCode != 0) return false;

            Power.Restart("Host joined to Active Directory, restart required", Power.ShutdownOptions.Delay);
            return true;
        }

        public bool UnRegisterComputer(DataContracts.HostnameChanger msg)
        {
            try
            {
                Domain.GetComputerDomain();
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                // If the host is not bound to a domain, this exception will be thrown
                return true;
            }
            catch (Exception)
            {
                // Swallow any unknown errors and proceed with the unbiding process
            }

            try
            {

                var returnCode = NetUnjoinDomain(null, msg.ADUser,
                    msg.ADPass, UnJoinOptions.NetsetupAccountDelete);

                Log.Entry(Name,
                    $"{(_returnCodes.ContainsKey(returnCode) ? $"{_returnCodes[returnCode]}, code = " : "Unknown Return Code: ")} {returnCode}");

                if (returnCode == 0)
                    Power.Restart("Host left active directory, restart needed", Power.ShutdownOptions.Delay);

                if (returnCode == 0 || returnCode == 2692)
                    return true;
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }

            return false;
        }

        public void ActivateComputer(string key)
        {
            Log.Entry(Name, "Checking Product Key Activation");
            if (key.Length == 25 && !key.Contains("-"))
            {
                var keyParts = new string[5];
                keyParts[0] = key.Substring(0, 5);
                keyParts[1] = key.Substring(5, 5);
                keyParts[2] = key.Substring(10, 5);
                keyParts[3] = key.Substring(15, 5);
                keyParts[4] = key.Substring(20, 5);
                key = string.Join("-", keyParts);
            }
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
