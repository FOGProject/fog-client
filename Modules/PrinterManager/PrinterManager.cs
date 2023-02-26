/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
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
    public class PrinterManager : AbstractModule<DataContracts.PrinterManager>
    {
        private static string LogName;
        private readonly PrintManagerBridge _instance;
        private readonly List<string> _configuredPrinters;

        public PrinterManager()
        {
            Compatiblity = Settings.OSType.Windows;
            Name = "PrinterManager";
            ShutdownFriendly = false;
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

        protected override void DoWork(Response data, DataContracts.PrinterManager msg)
        {
            if (msg.Mode == "0") return;

            var installedPrinters = _instance.GetPrinters();
            var printerAdded = false;

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
                if (installedPrinters.Contains(printer.Name))
                {
                    Log.Entry(Name, printer.Name + " already exists");
                    CleanPrinter(printer.Name);
                }
                else if (_configuredPrinters.Contains(printer.ToString()))
                {
                    Log.Entry(Name, printer.Name + " has already been configured");
                }
                else
                {
                    printerAdded = true;
                    printer.Add(_instance);
                    CleanPrinter(printer.Name);
                }
            }
            printerAdded = BatchConfigure(msg.Printers) || printerAdded;

            if(printerAdded)
                _instance.ApplyChanges();
        }

        private void RemoveExtraPrinters(List<Printer> newPrinters, DataContracts.PrinterManager msg, List<string> installedPrinters )
        {
            var managedPrinters = newPrinters.Where(printer => printer != null).Select(printer => printer.Name).ToList();

            if (!msg.Mode.Equals("ar", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var name in msg.AllPrinters.Where(name => !managedPrinters.Contains(name) && installedPrinters.Contains(name)))
                    CleanPrinter(name, true);
            }
            else
            {
                foreach (var name in installedPrinters.Where(name => !managedPrinters.Contains(name)))
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