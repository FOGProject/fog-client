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
using System.Linq;
using Zazzles;
using Zazzles.Modules;

// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace FOG.Modules.PrinterManager
{
    /// <summary>
    ///     Manage printers
    /// </summary>
    public sealed class PrinterManager : PolicyModule<PrinterMessage>
    {
        public override string Name { get; protected set; }
        public override Settings.OSType Compatiblity { get; protected set; }
        private readonly PrintManagerBridge _instance;

        public PrinterManager()
        {
            Compatiblity = Settings.OSType.Windows;
            Name = "PrinterManager";

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

        private void RemoveExtraPrinters(PrinterMessage message)
        {
            Log.Debug(Name, "Removing extra printers...");


            if (message.Level == ManagementLevel.All)
            {
                var printerNames = _instance.GetPrinters();
                foreach (var name in 
                    from name in printerNames
                    let found = message.Printers.Any(printer => printer.Name == name)
                    where !found select name)
                {
                    _instance.Remove(name);
                }
            }
            else
            {
                foreach (var printer in message.UnManagedPrinters.Where(printer => PrinterExists(printer.Name)))
                {
                    _instance.Remove(printer.Name);
                }
            }
        }

        private bool PrinterExists(string name)
        {
            try
            {
                return _instance.GetPrinters().Contains(name);
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not detect if printer exists");
                Log.Error(Name, ex);
            }

            return false;
        }

        protected override void OnEvent(PrinterMessage message)
        {
            RemoveExtraPrinters(message);

            Log.Entry(Name, "Adding printers");
            foreach (var printer in message.Printers)
            {
                if (!PrinterExists(printer.Name))
                    printer.Add(_instance);
                else
                    Log.Entry(Name, printer.Name + " already exists");
            }
        }
    }

    public enum ManagementLevel
    {
        ManagedOnly,
        All
    }
}