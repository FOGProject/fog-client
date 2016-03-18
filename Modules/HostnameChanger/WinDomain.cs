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
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using FOG.Handlers;
using FOG.Handlers.Middleware;

namespace FOG.Modules.HostnameChanger
{
    internal class WinDomain
    {
        private static string LogName = "WinDomain";

        public static readonly Dictionary<int, string> ReturnCodes = new Dictionary<int, string>
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
        private enum DomainOptions
        {
            NETSETUP_JOIN_DOMAIN = 0x00000001,
            NETSETUP_ACCT_CREATE = 0x00000002,
            NETSETUP_ACCT_DELETE = 0x00000004,
            NETSETUP_WIN9X_UPGRADE = 0x00000010,
            NETSETUP_DOMAIN_JOIN_IF_JOINED = 0x00000020,
            NETSETUP_JOIN_UNSECURE = 0x00000040,
            NETSETUP_MACHINE_PWD_PASSED = 0x00000080,
            NETSETUP_DEFER_SPN_SET = 0x00000100,
        }

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetJoinDomain(string lpServer, string lpDomain, string lpAccountOU,
            string lpAccount, string lpPassword, DomainOptions nameType);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetUnjoinDomain(string lpServer, string lpAccount, string lpPassword,
            DomainOptions fUnjoinOptions);


        public static int Join(Response response)
        {
            var returnCode = JoinDomain(response, true, (DomainOptions.NETSETUP_JOIN_DOMAIN | DomainOptions.NETSETUP_ACCT_CREATE));

            switch (returnCode)
            {
                case 2224:
                    returnCode = JoinDomain(response, true, DomainOptions.NETSETUP_JOIN_DOMAIN);
                    break;
                case 2:
                case 50:
                case 1355:
                    returnCode = JoinDomain(response, false, (DomainOptions.NETSETUP_JOIN_DOMAIN | DomainOptions.NETSETUP_ACCT_CREATE));
                    break;
            }

            return returnCode;
        }

        private static int JoinDomain(Response response, bool ou, DomainOptions options)
        {
            return NetJoinDomain(null,
                response.GetField("#ADDom"),
                ou ? response.GetField("#ADOU") : null,
                response.GetField("#ADUser"),
                response.GetField("#ADPass"),
                options);
        }

        public static int Leave(Response response)
        {
            return NetUnjoinDomain(null, 
                response.GetField("#ADUser"), 
                response.GetField("#ADPass"), 
                DomainOptions.NETSETUP_ACCT_DELETE);
        }

        public static IPAddress[] GetDomainIPAddresses()
        {
            using (var domain = Domain.GetComputerDomain())
            {
                return Dns.GetHostAddresses(domain.Name);
            }
        }

        public static bool InCorrectDomain(Response response)
        {
            try
            {
                var currentIP = WinDomain.GetDomainIPAddresses();
                var targetIP = Dns.GetHostAddresses(response.GetField("#ADDom"));

                return currentIP.Intersect(targetIP).Any();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could verify current domain");
                Log.Error(LogName, ex);
            }

            return false;
        }

    }
}