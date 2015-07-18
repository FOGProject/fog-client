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

        public static bool PinCert()
        {
            try
            {
                var cert = RSA.GetCACertificate();
                if (cert != null) return false;

                var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                Configuration.ServerAddress = Configuration.ServerAddress.Replace("https://", "http://");
                var keyPath = string.Format("{0}ca.cert.der", tempDirectory);
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
                throw;
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
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not save settings");
                Log.Error(LogName, ex);
                throw;
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
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could unpin CA cert");
                Log.Error(LogName, ex);
                throw;
            }
        }
    }
}
