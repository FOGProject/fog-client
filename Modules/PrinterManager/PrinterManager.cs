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
        private readonly List<string> _configuredPrinters;

        public PrinterManager()
        {
            Compatiblity = Settings.OSType.Windows;
            Name = "PrinterManager";
            LogName = Name;
            _configuredPrinters = new List<string>();

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
            Log.Entry(Name, "Getting installed printers");
            var installedPrinters = _instance.GetPrinters();

            var printerAdded = false;

            //Get printers
            if (msg.Mode == "0") return;

            if (data.Error && data.ReturnCode.Equals("np", StringComparison.OrdinalIgnoreCase))
            {
                RemoveExtraPrinters(new List<Printer>(), msg, installedPrinters);
                return;
            }
            if (data.Error) return;
            if (!data.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            RemoveExtraPrinters(msg.Printers, msg, installedPrinters);

            Log.Entry(Name, "Adding printers");
            foreach (var printer in msg.Printers)
            {
                if (!installedPrinters.Contains(printer.Name))
                {
                    printerAdded = true;
                    printer.Add(_instance);
                    CleanPrinter(printer.Name);
                }
                else
                {
                    Log.Entry(Name, printer.Name + " already exists");
                    CleanPrinter(printer.Name);
                }
            }
            printerAdded = printerAdded || BatchConfigure(msg.Printers);

            if(printerAdded)
                _instance.ApplyChanges();
        }

        private void RemoveExtraPrinters(List<Printer> newPrinters, PrinterMessage msg, List<string> existingPrinters )
        {
            var managedPrinters = newPrinters.Where(printer => printer != null).Select(printer => printer.Name).ToList();

            if (!msg.Mode.Equals("ar", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var name in msg.AllPrinters.Where(name => !managedPrinters.Contains(name) && existingPrinters.Contains(name)))
                    CleanPrinter(name, true);
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
                return printerList.Contains(name);
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not detect if printer exists");
                Log.Error(Name, ex);
            }

            return false;
        }


        private void CleanPrinter(string name, bool cleanOriginal = false)
        {
            var printerList = _instance.GetPrinters();

            const string copyWord = "(Copy";
            var matches = printerList.Where(printer => printer.Contains(copyWord)).ToList();

            if(cleanOriginal && printerList.Contains(name))
                matches.Add(name);

            foreach (var printer in matches.Select(match => new Printer { Name = match }))
            {
                printer.Remove(_instance);
            }
        }

        private bool BatchConfigure(List<Printer> printers)
        {
            var configuredAny = false;

            var allPrinters = new List<string>();
            var newPrinters = new List<string>();


            foreach (var printer in printers)
            {
                allPrinters.Add(printer.ToString());

                if (_configuredPrinters.Contains(printer.ToString()))
                    continue;

                newPrinters.Add(printer.ToString());

                try
                {
                    configuredAny = true;
                    _instance.Configure(printer);
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, "Unable to configure " + printer.Name);
                    Log.Error(LogName, ex);
                }
            }

            // Perform an except removal since _configuredPrinters is read only
            var extras = _configuredPrinters.Except(allPrinters);

            foreach (var extraPrinter in extras)
            {
                _configuredPrinters.Remove(extraPrinter);
            }

            _configuredPrinters.AddRange(newPrinters);

            return configuredAny;
        }
    }
}