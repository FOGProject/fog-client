using System;
using System.Security.Cryptography.X509Certificates;
using FOG.Handlers;
using FOG.Handlers.Data;

namespace CertificateManager
{
    class Program
    {
        private const string Name = "CertificateManager";

        static void Main(string[] args)
        {
            LogHandler.Output = LogHandler.Mode.Console;
            
            if (args.Length != 1) return;

            if (args[0].ToLower().Equals("install"))
                InstallCert();
            else if (args[0].ToLower().Equals("uninstall"))
                UninstallCert();
        }

        private static void InstallCert()
        {
            var cert = RSA.GetCACertificate();
            if (cert != null) return;


            CommunicationHandler.ServerAddress = CommunicationHandler.ServerAddress.Replace("https://", "http://");
            var keyPath = string.Format("{0}tmp\\ca.cert.der", AppDomain.CurrentDomain.BaseDirectory);
            var downloaded = CommunicationHandler.DownloadFile("/management/other/ca.cert.der", keyPath);
            if (!downloaded) return;

            LogHandler.Log(Name, "Installing FOG CA...");

            try
            {
                cert = new X509Certificate2(keyPath);
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();
            }
            catch (Exception ex)
            {
                LogHandler.Error(Name, ex);
             }
        }

        private static void UninstallCert()
        {
            var cert = RSA.GetCACertificate();
            if (cert == null) return;

            LogHandler.Log(Name, "Uninstalling FOG CA...");

            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(cert);
                store.Close();
            }
            catch (Exception ex)
            {
                LogHandler.Error(Name, ex);
            }
        }
    }
}
