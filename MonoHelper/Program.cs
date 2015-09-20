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
        private const string Location = "/opt/fog-service";

        private static void Main(string[] args)
        {
            if (args.Length != 6) return;

            var url = args[0];
            var tray = args[1];
            var version = args[2];
            var company = args[3];
            var rootLog = args[4];
            var https = args[5];

            var baseURL = url;
            var webRoot = "";

            try
            {
                if (url.Contains("/"))
                {
                    baseURL = url.Substring(0, url.IndexOf("/"));
                    webRoot = url.Substring(url.IndexOf("/"));
                }
            }
            catch (Exception)
            {
                // ignored
            }


            SaveSettings(https, tray, baseURL, webRoot, version, company, rootLog, Location);
            PinServerCert(Location);
        }

        public static bool PinServerCert(string location)
        {
            try
            {
                var cert = RSA.ServerCertificate();
                if (cert != null) return false;

                var keyPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "ca.cert.der");
                Settings.SetPath(Path.Combine(location + "settings.json"));
                Configuration.GetAndSetServerAddress();
                Configuration.ServerAddress = Configuration.ServerAddress.Replace("https://", "http://");

                var downloaded = Communication.DownloadFile("/management/other/ca.cert.der", keyPath);
                if (!downloaded)
                    return false;

                cert = new X509Certificate2(keyPath);

                return RSA.InjectCA(cert);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not pin CA");
                Log.Error(LogName, ex);
                return false;
            }
        }

        public static bool SaveSettings(string https, string usetray, string webaddress, string webroot, string version,
            string company, string rootLog, string location)
        {
            var filePath = Path.Combine(location, "settings.json");
            try
            {

                if (File.Exists(filePath))
                {
                    var settings = JObject.Parse(File.ReadAllText(filePath));
                    settings["Version"] = version;
                    File.WriteAllText(filePath, settings.ToString());
                }
                else
                {
                    var settings = new JObject
                    {
                        {"HTTPS", https},
                        {"Tray", usetray},
                        {"Server", webaddress},
                        {"WebRoot", webroot},
                        {"Version", version},
                        {"Company", company},
                        {"RootLog", rootLog}
                    };
                    File.WriteAllText(filePath, settings.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not save settings");
                Log.Error(LogName, ex);
                return false;
            }
        }

        public static bool UnpinServerCert()
        {
            var cert = RSA.ServerCertificate();
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