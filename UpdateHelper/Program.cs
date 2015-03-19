
using System;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;

namespace FOG {
	class Program {
		
		public static void Main(string[] args) {
			var service = new ServiceController("fogservice");
			//Update Line
			//Stop the service
			service.Stop();
			service.WaitForStatus(ServiceControllerStatus.Stopped);
			
			if( Process.GetProcessesByName("FOGService").Length > 0) {
				foreach(Process process in Process.GetProcessesByName("FOGService")) {
					process.Kill();
				}
			}
			
			applyUpdates();
			
			//Start the service

			service.Start();
			service.WaitForStatus(ServiceControllerStatus.Running);
			
			if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info"))
				File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\updating.info");
			
		}
		
		private static void applyUpdates() {
			var process = new Process();
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			
			process.StartInfo.FileName = "msiexec"; ;
			process.StartInfo.Arguments = "/i" + (AppDomain.CurrentDomain.BaseDirectory + @"\FOGService.msi") + "/quiet /L*V \"C:\fog.install.log\"";
			process.Start();
			process.WaitForExit();
		}
	}
}