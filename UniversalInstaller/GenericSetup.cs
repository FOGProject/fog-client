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
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FOG.Core;
using FOG.Core.Data;
using FOG.Core.Middleware;
using Newtonsoft.Json.Linq;

namespace FOG
{
    public static class GenericSetup
    {
        private const string LogName = "Installer";
        private const string ClientVersion = "0.10.0";

        public static IInstall Instance { get;  }

        static GenericSetup()
        {
            switch (Settings.OS)
            {
                case Settings.OSType.Mac:
                    Instance = new MacInstall();
                    break;
                case Settings.OSType.Linux:
                    Instance = new LinuxInstall();
                    break;
                default:
                    Instance = new WindowsInstall();
                    break;
            }
        }

        public static bool PinServerCert()
        {
            try
            {
                var cert = RSA.ServerCertificate();
                if (cert != null) return false;

                var keyPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "ca.cert.der");
                Settings.SetPath(Path.Combine(Instance.GetLocation(), "settings.json"));
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

        public static bool SaveSettings(string https, string usetray, string webaddress, string webroot,
            string company, string rootLog)
        {
            var filePath = Path.Combine(Instance.GetLocation(), "settings.json");
            try
            {

                if (File.Exists(filePath))
                {
                    var settings = JObject.Parse(File.ReadAllText(filePath));
                    settings["Version"] = ClientVersion;
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
                        {"Version", ClientVersion},
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

        public static void AdjustPermissions(string location)
        {
            var logLocation = Path.Combine(location, "fog.log");

            if (!File.Exists(logLocation))
                File.Create(logLocation);

            ProcessHandler.Run("chmod", "755 " + logLocation);
        }

        public static void ExtractFiles(string tmp, string location)
        {
            var tmpLocation = Path.Combine(tmp, "FOGService.zip");
            ExtractResource("FOGService.zip", tmpLocation);
            ZipFile.ExtractToDirectory(tmpLocation, location);
            File.Delete(tmpLocation);
        }

        private static void ExtractResource(string resource, string filePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var input = assembly.GetManifestResourceStream(resource))
            using (var output = File.Create(filePath))
            {
                CopyStream(input, output);
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}