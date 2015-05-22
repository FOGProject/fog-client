using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CertificateManager
{
    class Program
    {
        private const string CAName = "FOG Server CA";

        static void Main(string[] args)
        {
            if (args.Length != 1) return;

            if (args[0].ToLower().Equals("install"))
                InstallCert();
            else if (args[1].ToLower().Equals("uninstall"))
                UninstallCert();
        }

        private static void InstallCert()
        {
            var cert = GetCertFromStore();
            if (cert != null) return;

            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Certificates.Add(cert);

                store.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void UninstallCert()
        {
            var cert = GetCertFromStore();
            if (cert == null) return;

            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Certificates.Remove(cert);
                store.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static X509Certificate2 GetCertFromStore()
        {
            try
            {
                X509Certificate2 CAroot = null;
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var cers = store.Certificates.Find(X509FindType.FindBySubjectName, CAName, true);

                if (cers.Count > 0)
                {
                    CAroot = cers[0];
                }
                store.Close();

                return CAroot;
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }
    }
}
