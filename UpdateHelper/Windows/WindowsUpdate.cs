using System;
using System.Diagnostics;
using System.ServiceProcess;
using FOG.Handlers;

namespace FOG
{
    class WindowsUpdate : IUpdate
    {
        const string LogName = "UpdateHelper";

        public void ApplyUpdate()
        {
            var useTray = Settings.Get("Tray");
            var https = Settings.Get("HTTPS");
            var webRoot = Settings.Get("WebRoot");
            var server = Settings.Get("Server");
            var logRoot = Settings.Get("RootLog");

            var process = new Process
            {
                StartInfo =
                {
                    Arguments = string.Format("/i \"{0}\" /quiet USETRAY=\"{1}\" HTTPS=\"{2}\" WEBADDRESS=\"{3}\" WEBROOT=\"{4}\" ROOTLOG=\"{5}\"", 
                        (AppDomain.CurrentDomain.BaseDirectory + "FOGService.msi"), 
                        useTray, https, server, webRoot, logRoot)
                }
            };
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.StartInfo.FileName = "msiexec";

            Log.Entry(LogName, "--> " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
            process.Start();
            process.WaitForExit();
        }

        public void StartService()
        {
            using (var service = new ServiceController("fogservice"))
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);               
            }
        }

        public void StopService()
        {
            using (var service = new ServiceController("fogservice"))
            {
                if (service.Status == ServiceControllerStatus.Running)
                    service.Stop();

                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }
    }
}
