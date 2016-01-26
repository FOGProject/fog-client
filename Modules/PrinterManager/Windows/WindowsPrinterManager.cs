﻿/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2015 FOG Project
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
    internal class WindowsPrinterManager : PrintManagerBridge
    {
        private const string LogName = "PrinterManager";

        public override List<string> GetPrinters()
        {
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            return
                (from ManagementBaseObject printer in printerQuery.Get()
                    select printer.GetPropertyValue("name").ToString()).ToList();
        }

        protected override void AddiPrint(iPrintPrinter printer)
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

        protected override void AddLocal(LocalPrinter printer)
        {
            if (printer.IP != null)
                AddIPPort(printer, "9100");

            var proc = Process.Start("rundll32.exe",
                $" printui.dll,PrintUIEntry /if /q /b \"{printer.Name}\" /f \"{printer.File}\" /r \"{printer.Port}\" /m \"{printer.Model}\"");
            proc?.WaitForExit(120000);
        }

        protected override void AddNetwork(NetworkPrinter printer)
        {
            // Add per machine printer connection
            var proc = Process.Start("rundll32.exe", " printui.dll,PrintUIEntry /ga /n " + printer.Name);
            proc?.WaitForExit(120000);
            // Add printer network connection, download the drivers from the print server
            proc = Process.Start("rundll32.exe", " printui.dll,PrintUIEntry /in /n " + printer.Name);
            proc?.WaitForExit(120000);
        }

        protected override void AddCUPS(CUPSPrinter printer)
        {
            throw new NotImplementedException();
        }

        public override void Remove(string name)
        {
            Log.Entry("Printer", "Removing printer: " + name);
            var proc = name.StartsWith("\\\\")
                ? Process.Start("rundll32.exe", $" printui.dll,PrintUIEntry /gd /q /n \"{name}\"")
                : Process.Start("rundll32.exe", $" printui.dll,PrintUIEntry /dl /q /n \"{name}\"");

            if (proc == null) return;
            proc.Start();
            proc.WaitForExit(120000);
        }

        public override void Default(string name)
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
            var collection = searcher.Get();

            foreach (
                var currentObject in
                    collection.Cast<ManagementObject>()
                        .Where(currentObject => currentObject["name"].ToString().Equals(name)))
                currentObject.InvokeMethod("SetDefaultPrinter", new object[] {name});
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
