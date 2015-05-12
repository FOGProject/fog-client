using System.Diagnostics;
using FOG.Handlers;

namespace FOG.Modules
{
    class iPrintPrinter : Printer
    {
        public iPrintPrinter(string name, string ip, string port, bool defaulted)
        {
            Name = name;
            IP = ip;
            Port = port;
            Default = defaulted;
            LogName = "iPrinter";
        }

        public override void Add()
        {
            LogHandler.Log(LogName, "Attempting to add printer:");
            LogHandler.Log(LogName, string.Format("--> Name = {0}", Name));
            LogHandler.Log(LogName, string.Format("--> Port = {0}", Port));

            var proc = new Process
            {
                StartInfo =
                {
                    FileName = @"c:\windows\system32\iprntcmd.exe",
                    Arguments = " -a no-gui \"" + Port + "\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            proc.Start();
            proc.WaitForExit(120000);
        }
    }
}
