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

// ReSharper disable InconsistentNaming

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace FOG.Handlers.Middleware
{
    public static class Configuration
    {
        private const string LogName = "Middleware::Configuration";
        public static string ServerAddress { get; set; }
        public static string TestMAC { get; set; }

        static Configuration()
        {
            GetAndSetServerAddress();
        }

        /// <summary>
        ///     Load the server information from the registry and apply it
        ///     <returns>True if settings were updated</returns>
        /// </summary>
        public static bool GetAndSetServerAddress()
        {

            if (RegistryHandler.GetSystemSetting("HTTPS") == null || RegistryHandler.GetSystemSetting("WebRoot") == null ||
                string.IsNullOrEmpty(RegistryHandler.GetSystemSetting("Server")))
            {
                Log.Error(LogName, "Invalid parameters");
                return false;
            }

            ServerAddress = (RegistryHandler.GetSystemSetting("HTTPS").Equals("1") ? "https://" : "http://");
            ServerAddress += RegistryHandler.GetSystemSetting("Server") +
                             RegistryHandler.GetSystemSetting("WebRoot");
            return true;
        }

        /// <summary>
        ///     Get the IP address of the host
        ///     <returns>The first IP address of the host</returns>
        /// </summary>
        public static string IPAddress()
        {
            var hostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(hostName);
            var address = ipEntry.AddressList;

            return (address.Length > 0) ? address[0].ToString() : "";
        }

        /// <summary>
        ///     Get a string of all the host's valid MAC addresses
        ///     <returns>A string of all the host's valid MAC addresses, split by |</returns>
        /// </summary>
        public static string MACAddresses()
        {
            if (!string.IsNullOrEmpty(TestMAC)) return TestMAC;

            var macs = "";
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();

                macs = adapters.Aggregate(macs, (current, adapter) =>
                    current + ("|" + string.Join(":", (from z in adapter.GetPhysicalAddress().GetAddressBytes() select z.ToString("X2")).ToArray())));

                // Remove the first |
                if (macs.Length > 0)
                    macs = macs.Substring(1);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not get MAC addresses");
                Log.Error(LogName, ex);
            }

            return macs;
        }
    }
}