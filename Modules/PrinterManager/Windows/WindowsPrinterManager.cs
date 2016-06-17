/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2016 FOG Project
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using Zazzles;

namespace FOG.Modules.PrinterManager
{
    public class WindowsPrinterManager : PrintManagerBridge
    {
        private const string LogName = "PrinterManager";

        private void PrintUI(string cmdLine, bool verbose = false)
        {
            if (!verbose)
                cmdLine = cmdLine + " /q";

            try
            {
                using (var proc = Process.Start("rundll32.exe", $" printui.dll,PrintUIEntry {cmdLine}"))
                {
                    proc.WaitForExit(30*1000);

                    if (proc.HasExited)
                    {
                        Log.Entry(LogName, "PrintUI return code = " + proc.ExitCode);
                    }
                    else
                    {
                        Log.Entry(LogName, "PrintUI has not finished in a timely fashion, abandoning process");

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Failed on PrintUI " + cmdLine);
                Log.Error(LogName, ex);
            }
        }

        public override List<string> GetPrinters()
        {
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            return
                (from ManagementBaseObject printer in printerQuery.Get()
                    select printer.GetPropertyValue("name").ToString()).ToList();
        }

        protected override void AddiPrint(Printer printer, bool verbose = false)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = @"c:\windows\system32\iprntcmd.exe",
                    Arguments = " -a no-gui \"" + printer.Port + "\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            proc.Start();
            proc.WaitForExit(120000);
            Log.Entry(LogName, "Return code " + proc.ExitCode);
        }

        protected override void AddLocal(Printer printer, bool verbose = false)
        {
            if (printer.IP != null)
                AddIPPort(printer, "9100");

            PrintUI($"/if /b \"{printer.Name}\" /f \"{printer.File}\" /r \"{printer.Port}\" /m \"{printer.Model}\"", verbose);
        }

        protected override void AddNetwork(Printer printer, bool verbose = false)
        {
            PrintUI($"/ga /n \"{printer.Name}\"", verbose);
        }

        protected override void AddCUPS(Printer printer, bool verbose = false)
        {
            throw new NotImplementedException();
        }

        public override void Remove(string name, bool verbose = false)
        {
            if (name.StartsWith("\\\\"))
            {
                PrintUI($"/dn /n \"{name}\"", verbose);
                PrintUI($"/gd /n \"{name}\"", verbose);
            }
            else
            {
                PrintUI($"/dl /q /n \"{name}\"", verbose);
            }
        }

        public override void Default(string name, bool verbose = false)
        {
            PrintUI($"/y /n \"{name}\"", verbose);
        }

        public override void Configure(Printer printer, bool verbose = false)
        {
            if (printer.Type == Printer.PrinterType.Network)
            {
                Log.Entry(LogName, $"Invoking add {printer.Name} for all users");
                PrintUI($"/in /n \"{printer.Name}\"", verbose);
            }

            if (string.IsNullOrEmpty(printer.ConfigFile)) return;

            Log.Entry(LogName, "Configuring " + printer.Name);
            PrintUI($"/Sr /n \"{printer.Name}\" /a \"{printer.ConfigFile}\" m f g p", verbose);
        }

        public override void ApplyChanges()
        {
            Log.Entry(LogName, "Restarting spooler");
            Process.Start("net", "stop spooler")?.WaitForExit(15*1000);
            Process.Start("net", "start spooler")?.WaitForExit(15 * 1000);
        }

        private void AddIPPort(Printer printer, string remotePort)
        {
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

            try
            {
                if (printer.IP != null && printer.IP.Contains(":"))
                {
                    var arIP = printer.IP.Split(':');
                    if (arIP.Length == 2)
                        remotePort = int.Parse(arIP[1]).ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not parse port from IP");
                Log.Error(LogName, ex);
            }

            mPort.SetPropertyValue("Name", printer.Port);
            mPort.SetPropertyValue("Protocol", 1);
            mPort.SetPropertyValue("HostAddress", printer.IP);
            mPort.SetPropertyValue("PortNumber", remotePort);
            mPort.SetPropertyValue("SNMPEnabled", false);

            var put = new PutOptions
            {
                UseAmendedQualifiers = true,
                Type = PutType.UpdateOrCreate
            };
            mPort.Put(put);
        }
    }
}
