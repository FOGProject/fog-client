using System.Diagnostics;
using FOG.Handlers;
using FOG.Handlers.Middleware;

namespace FOG.Modules.HostnameChanger.Mac
{
    class MacHostName : IHostName
    {
        private string Name = "HostnameChanger";

        public void RenameComputer(string hostname)
        {

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "scutil",
                    Arguments = "--set HostName " + hostname,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                Log.Entry(Name, "Changing hostname");
                process.Start();
                process.WaitForExit();
                Log.Entry(Name, "Return code = " + process.ExitCode);

                Log.Entry(Name, "Changing Bonjour name");
                process.StartInfo.Arguments = "--set LocalHostName " + hostname;
                process.Start();
                process.WaitForExit();
                Log.Entry(Name, "Return code = " + process.ExitCode);

                Log.Entry(Name, "Changing Computer name");
                process.StartInfo.Arguments = "--set ComputerName " + hostname;
                process.Start();
                process.WaitForExit();
                Log.Entry(Name, "Return code = " + process.ExitCode);
            }
        }

        public bool RegisterComputer(Response response)
        {
            throw new System.NotImplementedException();
        }

        public void UnRegisterComputer(Response response)
        {
            throw new System.NotImplementedException();
        }

        public void ActivateComputer(string key)
        {
            throw new System.NotImplementedException();
        }
    }
}
