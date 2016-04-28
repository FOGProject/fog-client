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
using Zazzles;

namespace FOG.Modules.PrinterManager
{
    public abstract class PrintManagerBridge
    {
        private const string LogName = "Printer";
        public abstract List<string> GetPrinters();

        public void Add(Printer printer)
        {
            Log.Entry(LogName, "Attempting to add printer:");

            if (printer.Name != null)
                Log.Entry(LogName, $"--> Name = {printer.Name}");
            if (printer.IP != null)
                Log.Entry(LogName, $"--> IP = {printer.IP}");
            if (printer.Port != null)
                Log.Entry(LogName, $"--> Port = {printer.Port}");
            if (printer.File != null)
                Log.Entry(LogName, $"--> File = {printer.File}");
            if (printer.ConfigFile != null)
                Log.Entry(LogName, $"--> Config = {printer.ConfigFile}");
            if (printer.Model != null)
                Log.Entry(LogName, $"--> Model = {printer.Model}");

            try
            {
                switch (printer.Type)
                {
                    case Printer.PrinterType.iPrint:
                        AddiPrint(printer);
                        break;
                    case Printer.PrinterType.Local:
                        AddLocal(printer);
                        break;
                    case Printer.PrinterType.Network:
                        AddNetwork(printer);
                        break;
                    case Printer.PrinterType.CUPS:
                        AddCUPS(printer);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not add");
                Log.Error(LogName, ex);
            }
        }

        protected abstract void AddiPrint(Printer printer);
        protected abstract void AddLocal(Printer printer);
        protected abstract void AddNetwork(Printer printer);
        protected abstract void AddCUPS(Printer printer);
        public abstract void Remove(string name);
        public abstract void Default(string name);
    }
}