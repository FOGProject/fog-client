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
using Zazzles;

namespace FOG.Modules.PrinterManager
{
    public abstract class PrintManagerBridge
    {
        private const string LogName = "Printer";
        public abstract List<string> GetPrinters();

        public void Add(Printer printer, bool verbose = false)
        {
            try
            {
                switch (printer.Type)
                {
                    case Printer.PrinterType.iPrint:
                        AddiPrint(printer, verbose);
                        break;
                    case Printer.PrinterType.Local:
                        AddLocal(printer,verbose);
                        break;
                    case Printer.PrinterType.Network:
                        AddNetwork(printer, verbose);
                        break;
                    case Printer.PrinterType.CUPS:
                        AddCUPS(printer, verbose);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not add");
                Log.Error(LogName, ex);
            }
        }

        protected abstract void AddiPrint(Printer printer, bool verbose = false);
        protected abstract void AddLocal(Printer printer, bool verbose = false);
        protected abstract void AddNetwork(Printer printer, bool verbose = false);
        protected abstract void AddCUPS(Printer printer, bool verbose = false);
        public abstract void Remove(string name, bool verbose = false);
        public abstract void Default(string name, bool verbose = false);
        public abstract void Configure(Printer printer, bool verbose = false);
        public abstract void ApplyChanges();
    }
}