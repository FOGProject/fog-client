using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FOG.Handlers.Data;
using FOG.Handlers.Middleware;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32.TaskScheduler;
 
namespace SetupHelper
{
    public class CustomActions
    {
        static void DisplayMSIError(Session session, string msg)
        {
            var r = new Record();
            r.SetString(0, msg);
            session.Message(InstallMessage.Error, r);
        }

        [CustomAction]
        public static ActionResult InstallCert(Session session)
        {
            var cert = RSA.GetCACertificate();
            if (cert != null) return ActionResult.Success;

            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Configuration.ServerAddress = Configuration.ServerAddress.Replace("https://", "http://");
            var keyPath = string.Format("{0}ca.cert.der", tempDirectory);
            var downloaded = Communication.DownloadFile("/management/other/ca.cert.der", keyPath);
            if (!downloaded)
            {
                DisplayMSIError(session, "Failed to download CA certificate");
                return ActionResult.Failure;
            }

            try
            {
                cert = new X509Certificate2(keyPath);
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to install CA certificate: " + ex.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult UninstallCert(Session session)
        {
            var cert = RSA.GetCACertificate();
            if (cert == null) return ActionResult.Success;

            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(cert);
                store.Close();
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to remove CA certficate: " + ex.Message);
                return ActionResult.Success;
            }
            
        }

        [CustomAction]
        public static ActionResult InstallFOGCert(Session session)
        {
            try
            {
                var cert = new X509Certificate2(session.CustomActionData["CAFile"]);
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to install FOG Project CA: " + ex.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult UninstallFOGCert(Session session)
        {
            var cert = new X509Certificate2();
            try
            {
                X509Certificate2 CAroot = null;
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var cers = store.Certificates.Find(X509FindType.FindBySubjectName, "FOG Project", true);

                if (cers.Count > 0)
                {
                    CAroot = cers[0];
                }
                store.Close();

                cert = CAroot;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to remove FOG Project CA certficate: " + ex.Message);
                return ActionResult.Success;
            }

            if (cert == null) return ActionResult.Success;

            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(cert);
                store.Close();
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                DisplayMSIError(session, "Unable to remove CA certficate: " + ex.Message);
                return ActionResult.Success;
            }

        }


        [CustomAction]
        public static ActionResult CleanTasks(Session session)
        {
            try
            {
                var taskService = new TaskService();
                var existingTasks = taskService.GetFolder("FOG").AllTasks.ToList();

                foreach (var task in existingTasks)
                    taskService.RootFolder.DeleteTask(@"FOG\" + task.Name);

                taskService.RootFolder.DeleteFolder("FOG", false);
            }
            catch (Exception ) { }

            return ActionResult.Success;
        }
    }
}
