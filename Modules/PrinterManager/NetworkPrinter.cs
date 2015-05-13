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
            LogHandler.Log(LogName, "Attempting to add printer:");
            LogHandler.Log(LogName, string.Format("--> Name = {0}", Name));
            LogHandler.Log(LogName, string.Format("--> IP = {0}", IP));
            LogHandler.Log(LogName, string.Format("--> Port = {0}", Port));

            if (string.IsNullOrEmpty(IP) || !Name.StartsWith("\\\\")) return;

            if (IP.Contains(":"))
            {
                var arIP = IP.Split(':');
                if (arIP.Length == 2)
                {
                    IP = arIP[0];
                    Port = arIP[1];
                }
            }

            var conn = new ConnectionOptions
            {
                EnablePrivileges = true,
                Impersonation = ImpersonationLevel.Impersonate
            };

            var mPath = new ManagementPath("Win32_TCPIPPrinterPort");

            var mScope = new ManagementScope(@"\\.\root\cimv2", conn)
            {
                Options =
                {
                    EnablePrivileges = true,
                    Impersonation = ImpersonationLevel.Impersonate
                }
            };

            var mPort = new ManagementClass(mScope, mPath, null).CreateInstance();

            if (mPort != null)
            {
                mPort.SetPropertyValue("Name", "IP_" + IP);
                mPort.SetPropertyValue("Protocol", 1);
                mPort.SetPropertyValue("HostAddress", IP);
                mPort.SetPropertyValue("PortNumber", Port);
                mPort.SetPropertyValue("SNMPEnabled", false);

                var put = new PutOptions
                {
                    UseAmendedQualifiers = true,
                    Type = PutType.UpdateOrCreate
                };
                mPort.Put(put);
            }

            if (!Name.StartsWith("\\\\")) return;

            // Add per machine printer connection
            var proc = Process.Start("rundll32.exe", " printui.dll,PrintUIEntry /ga /n " + Name);
            if (proc != null) proc.WaitForExit(120000);
            // Add printer network connection, download the drivers from the print server
            proc = Process.Start("rundll32.exe", " printui.dll,PrintUIEntry /in /n " + Name);
            if (proc != null) proc.WaitForExit(120000);
        }
    }
}
