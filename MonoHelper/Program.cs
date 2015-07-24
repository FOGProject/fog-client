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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using FOG.Handlers;
using FOG.Handlers.Data;
using FOG.Handlers.Middleware;
using Newtonsoft.Json.Linq;

namespace FOG
{
    public static class MonoHelper
    {
        private const string LogName = "Installer";

        static void Main(string[] args)
        {
        }

        public static bool PinCert(string location)
        {
            try
            {
                var cert = RSA.GetCACertificate();
                if (cert != null) return false;

                var keyPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "ca.cert.der");
                Settings.SetPath(Path.Combine(location + "settings.json"));
                Configuration.GetAndSetServerAddress();
                Configuration.ServerAddress = Configuration.ServerAddress.Replace("https://", "http://");
                var downloaded = Communication.DownloadFile("/management/other/ca.cert.der", keyPath);
                if (!downloaded)
                    return false;

                cert = new X509Certificate2(keyPath);
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not pin CA");
                Log.Error(LogName, ex);
                return false;
            }
        }

        public static bool SaveSettings(string https, string usetray, string webaddress, string webroot, string version, string company, string location, string rootlog)
        {
            try
            {
                var settings = new JObject
                {
                    {"HTTPS", https},
                    {"Tray", usetray},
                    {"Server", webaddress},
                    {"WebRoot", webroot},
                    {"Version", version},
                    {"Company", company},
                    {"RootLog", rootlog}
                };

                File.WriteAllText(Path.Combine(location, "settings.json"), settings.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not save settings");
                Log.Error(LogName, ex);
                return false;
            }

        }

        public static bool UnpinCert()
        {
            var cert = RSA.GetCACertificate();
            if (cert == null) return false;

            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(cert);
                store.Close();
                return true; 
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could unpin CA cert");
                Log.Error(LogName, ex);
                return false;
            }
        }
    }
}
