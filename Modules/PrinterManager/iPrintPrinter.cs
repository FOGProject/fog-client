using System.Diagnostics;
using FOG.Handlers;

namespace FOG.Modules.PrinterManager
{
    // ReSharper disable once InconsistentNaming
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
            Log.Entry(LogName, "Attempting to add printer:");
            Log.Entry(LogName, string.Format("--> Name = {0}", Name));
            Log.Entry(LogName, string.Format("--> Port = {0}", Port));

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
