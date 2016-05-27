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
using System.Linq;
using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;

// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace FOG.Modules.PrinterManager
{
    /// <summary>
    ///     Manage printers
    /// </summary>
    public class PrinterManager : AbstractModule<PrinterMessage>
    {
        private static string LogName;
        private readonly PrintManagerBridge _instance;

        public PrinterManager()
        {
            Compatiblity = Settings.OSType.Windows;
            Name = "PrinterManager";
            LogName = Name;

            switch (Settings.OS)
            {
                case Settings.OSType.Windows:
                    _instance = new WindowsPrinterManager();
                    break;
                default:
                    _instance = new UnixPrinterManager();
                    break;
            }
        }

        protected override void DoWork(Response data, PrinterMessage msg)
        {
            //Get printers
            if (msg.Mode.Equals("0")) return;

            if (data.Error && data.ReturnCode.Equals("np"))
            {
                RemoveExtraPrinters(new List<Printer>(), msg);
                return;
            }
            if (data.Error) return;
            if (!data.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            RemoveExtraPrinters(msg.Printers, msg);

            Log.Entry(Name, "Adding printers");
            foreach (var printer in msg.Printers)
            {
                if (!PrinterExists(printer.Name))
                    printer.Add(_instance);
                else
                    Log.Entry(Name, printer.Name + " already exists");
            }
        }

        private void RemoveExtraPrinters(List<Printer> newPrinters, PrinterMessage msg)
        {
            Log.Debug(Name, "Removing extra printers...");

            Log.Debug(Name, "Stripping printer data");

            var managedPrinters = new List<string>();
            foreach (var printer in newPrinters.Where(printer => printer != null))
            {
                Log.Debug(Name, "Stripping " + printer.Name);
                managedPrinters.Add(printer.Name);
            }

            if (!msg.Mode.Equals("ar"))
            {
                foreach (var name in msg.AllPrinters.Where(name => !managedPrinters.Contains(name) && PrinterExists(name)))
                    _instance.Remove(name);
            }
            else
            {
                var printerNames = _instance.GetPrinters();
                foreach (var name in printerNames.Where(name => !managedPrinters.Contains(name)))
                    _instance.Remove(name);
            }
        }

        private bool PrinterExists(string name)
        {
            try
            {
                var printerList = _instance.GetPrinters();
                if (printerList.Contains(name))
                    return true;

                const string copyWord = "(Copy";
                if (printerList.Where(printer => printer.Contains(copyWord))
                    .Select(printer => printer.Substring(0, printer.IndexOf(copyWord)).Trim())
                    .Any(rawName => rawName.Equals(name)))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not detect if printer exists");
                Log.Error(Name, ex);
            }

            return false;
        }
    }
}