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

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Zazzles;
using Zazzles.Modules;

// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace FOG.Modules.PrinterManager
{
    /// <summary>
    ///     Manage printers
    /// </summary>
    public sealed class DefaultPrinterManager : PolicyModule<PrinterMessage>
    {
        public override string Name { get; protected set; }
        public override Settings.OSType Compatiblity { get; protected set; }

        private readonly PrintManagerBridge _instance;

        public DefaultPrinterManager()
        {
            Name = "PrinterManager";
            Compatiblity = Settings.OSType.All;

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

        protected override void OnEvent(PrinterMessage message)
        {
            Log.Entry(Name, "Creating list of printers");
            var printerIDs = data["printers"].ToObject<List<string>>();
            Log.Entry(Name, "Creating printer objects");
            var printers = PrinterManager.CreatePrinters(printerIDs);

            Log.Entry(Name, "Checking defaults");
            foreach (var printer in printers.Where(printer => printer.Default))
                printer.SetDefault(_instance);
        }
    }
}