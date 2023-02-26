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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;
using Zazzles;
using Zazzles.Data;
using Zazzles.Middleware;

namespace FOG
{
    public static class GenericSetup
    {
        private const string LogName = "Installer";

        public static bool PinServerCert(string location, bool presetSettings = false)
        {

            if (!presetSettings)
                Settings.SetPath(Path.Combine(location, "settings.json"));

            Configuration.GetAndSetServerAddress();

            return PinServerCertPreset(location);
        }

        public static bool PinServerCert(string https, string address, string webroot, string location)
        {
            var useHTTPS = (https == "1");
            Configuration.ServerAddress = ((useHTTPS) ? "https://" : "http://") + address + webroot;
            return PinServerCertPreset(location);
        }


        private static bool PinServerCertPreset(string location)
        {
            try
            {
                var keyPath = Path.Combine(location, "ca.cert.der");
                var downloaded = Communication.DownloadFile("/management/other/ca.cert.der", keyPath);
                if (!downloaded)
                {
                    Log.Entry(LogName, "Could not download server CA cert from FOG server");
                    return false;
                }

                if (Settings.OS != Settings.OSType.Windows)
                {
                    Log.Entry(LogName, "Successfully pinned server CA cert stored in " + keyPath);
                    return true;
                }

                var cert = RSA.ServerCertificate();
                if (cert != null) UnpinServerCert();
                cert = new X509Certificate2(keyPath);

                var status = RSA.InjectCA(cert);
                if (status)
                    Log.Entry(LogName, "Successfully pinned server CA cert to " + cert.Subject);
                else
                    Log.Error(LogName, "Could not pin server CA to " + cert.Subject);
                return status;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not pin server CA");
                Log.Error(LogName, ex);
                throw;
            }
        }

        public static bool SaveSettings(string https, string usetray, string webaddress, string webroot,
            string company, string rootLog, string version, string location)
        {
            var filePath = Path.Combine(location, "settings.json");
            try
            {
                var settings = new JObject
                {
                    { "HTTPS", https},
                    { "Tray", usetray},
                    { "Server", webaddress},
                    { "WebRoot", webroot},
                    { "Version", version},
                    { "Company", company},
                    { "RootLog", rootLog}
                };
                File.WriteAllText(filePath, settings.ToString());
                Log.Entry(LogName, "Settings successfully saved in " + filePath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not save settings");
                Log.Error(LogName, ex);
                throw;
            }
        }

        public static bool UnpinServerCert()
        {
            if (Settings.OS != Settings.OSType.Windows)
                return true;

            var cert = RSA.ServerCertificate();
            if (cert == null) return false;

            try
            {
                var store = new X509Store(StoreName.Root, GetCertStoreLocation());
                store.Open(OpenFlags.ReadWrite);
                store.Remove(cert);
                store.Close();
                Log.Entry(LogName, "FOG server CA cert successfully unpinned");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not unpin FOG server CA cert");
                Log.Error(LogName, ex);
                throw;
            }
        }

        public static bool InstallFOGCert(string location)
        {
            // No need to install the FOG Project CA cert on non Windows systems
            // as this is only needed for auto-updating the fog-client on Windows
            if (Settings.OS != Settings.OSType.Windows)
                return true;

            try
            {
                var cert = new X509Certificate2(location);
                var store = new X509Store(StoreName.Root, GetCertStoreLocation());
                store.Open(OpenFlags.ReadWrite);
                var cers = store.Certificates.Find(X509FindType.FindBySubjectName, "FOG Project CA", true);

                var validKeyPresent = false;
                if (cers.Count > 0)
                {
                    for (var i = 0; i < cers.Count; i++)
                    {
                        X509Certificate2 CAroot = cers[i];
                        if (CAroot.GetCertHash() != cert.GetCertHash())
                            store.Remove(cert);
                        else
                            validKeyPresent = true;
                    }
                }

                if (!validKeyPresent)
                {
                    store.Add(cert);
                }
                store.Close();

                Log.Entry(LogName, "FOG Project CA successfully installed");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to install FOG Project CA cert");
                Log.Error(LogName, ex);
                throw;
            }
        }

        public static void UninstallFOGCert()
        {
            // No need to uninstall the FOG Project CA cert on non Windows systems
            // as this only exists on Windows, see install function above
            if (Settings.OS != Settings.OSType.Windows)
                return;

            var cert = new X509Certificate2();
            try
            {
                X509Certificate2 CAroot = null;
                var store = new X509Store(StoreName.Root, GetCertStoreLocation());
                store.Open(OpenFlags.ReadOnly);
                var cers = store.Certificates.Find(X509FindType.FindBySubjectName, "FOG Project CA", true);

                if (cers.Count > 0)
                {
                    CAroot = cers[0];
                }
                store.Close();

                cert = CAroot;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not find FOG Project CA cert");
                Log.Error(LogName, ex);
                throw;
            }

            if (cert == null) return;

            try
            {
                var store = new X509Store(StoreName.Root, GetCertStoreLocation());
                store.Open(OpenFlags.ReadWrite);
                store.Remove(cert);
                store.Close();
                Log.Entry(LogName, "FOG Project CA cert successfully uninstalled");
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not remove FOG Project CA cert");
                Log.Error(LogName, ex);
                throw;
            }

        }

        /// <summary>
        /// OS specific X509 Store location
        /// </summary>
        /// <returns>OS specific X509 StoreLocation</returns>
        private static StoreLocation GetCertStoreLocation()
        {
            switch (Settings.OS)
            {
                case Settings.OSType.Windows:
                    return StoreLocation.LocalMachine;
                case Settings.OSType.Nix:
                    return StoreLocation.LocalMachine;
                case Settings.OSType.Mac:
                    return StoreLocation.CurrentUser;
                case Settings.OSType.Linux:
                    return StoreLocation.LocalMachine;
                default:
                    return StoreLocation.LocalMachine;
            }
        }

    }
}