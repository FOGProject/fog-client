
using System;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;

namespace FOG {
	class Program {
		
		public static void Main(string[] args) {
			var service = new ServiceController("fogservice");
			String LOG_NAME = "Update Helper";
			
			LogHandler.Log(LOG_NAME, "Shutting down service...");
			//Stop the service
			if(service.Status == ServiceControllerStatus.Running)
				service.Stop();
			
			service.WaitForStatus(ServiceControllerStatus.Stopped);
			
			LogHandler.Log(LOG_NAME, "Killing remaining FOG processes...");
			if( Process.GetProcessesByName("FOGService").Length > 0) {
				foreach(Process process in Process.GetProcessesByName("FOGService")) {
					process.Kill();
				}
			}
			
			LogHandler.Log(LOG_NAME, "Applying MSI...");
			applyUpdates();
			
			//Start the service

			LogHandler.Log(LOG_NAME, "Starting service...");
			service.Start();
			service.WaitForStatus(ServiceControllerStatus.Running);
			service.Dispose();
			
			if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info"))
				File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info");
			
		}
		
		private static void applyUpdates() {
			String LOG_NAME = "Update Helper";
			var process = new Process();
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			
			process.StartInfo.FileName = "msiexec"; ;
			process.StartInfo.Arguments = "/i \"" + (AppDomain.CurrentDomain.BaseDirectory + "FOGService.msi") + "\" /quiet";
			LogHandler.Log(LOG_NAME, "--> " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
			process.Start();
			process.WaitForExit();
		}
	}
}