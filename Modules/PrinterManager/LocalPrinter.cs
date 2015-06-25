using System.Diagnostics;
using System.Management;
using FOG.Handlers;

namespace FOG.Modules.PrinterManager
{
    class LocalPrinter : Printer
    {
        public LocalPrinter(string name, string file, string port, string ip, string model, bool defaulted)
        {
            Name = name;
            Port = port;
            File = file;
            Model = model;
            Default = defaulted;
            IP = ip;
            LogName = "LocalPrinter";
        }

        public override void Add()
        {
            Log.Entry(LogName, "Attempting to add printer:");
            Log.Entry(LogName, string.Format("--> Name = {0}", Name));
            Log.Entry(LogName, string.Format("--> Port = {0}", Port));
            Log.Entry(LogName, string.Format("--> IP = {0}", IP));
            Log.Entry(LogName, string.Format("--> File = {0}", File));
            Log.Entry(LogName, string.Format("--> Model = {0}", Model));

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

            mPort.SetPropertyValue("Name", Port);
            mPort.SetPropertyValue("Protocol", 1);
            mPort.SetPropertyValue("HostAddress", IP);
            mPort.SetPropertyValue("PortNumber", 9100);
            mPort.SetPropertyValue("SNMPEnabled", false);

            var put = new PutOptions
            {
                UseAmendedQualifiers = true,
                Type = PutType.UpdateOrCreate
            };
            mPort.Put(put);

            var proc = Process.Start("rundll32.exe", 
                string.Format(" printui.dll,PrintUIEntry /if /q /b \"{0}\" /f \"{1}\" /r \"{2}\" /m \"{3}\"", Name, File, Port, Model));
            if (proc != null) proc.WaitForExit(120000);
        }
    }
}
