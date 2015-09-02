using System.Diagnostics;
using System.Management;
using FOG.Handlers;

namespace FOG.Modules.PrinterManager
{
    class NetworkPrinter : Printer
    {
        public NetworkPrinter(string name, string ip, string port, bool defaulted)
        {
            Name = name;
            IP = ip;
            Port = port;
            Default = defaulted;
            LogName = "NetworkPrinter";
        }

        public override void Add()
        {
            Log.Entry(LogName, "Attempting to add printer:");
            Log.Entry(LogName, string.Format("--> Name = {0}", Name));

            // Add per machine printer connection
            var proc = Process.Start("rundll32.exe", " printui.dll,PrintUIEntry /ga /n " + Name);
            proc?.WaitForExit(120000);
            Log.Entry(LogName, "Return code " + proc.ExitCode);
            // Add printer network connection, download the drivers from the print server
            proc = Process.Start("rundll32.exe", " printui.dll,PrintUIEntry /in /n " + Name);
            proc?.WaitForExit(120000);
            Log.Entry(LogName, "Return code " + proc.ExitCode);
        }
    }
}
