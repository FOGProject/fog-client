/*
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
using System.Linq;
using System.Management;
using FOG.Handlers;
using FOG.Handlers.Middleware;

// ReSharper disable ParameterTypeCanBeEnumerable.Local


namespace FOG.Modules.PrinterManager
{
    /// <summary>
    ///     Manage printers
    /// </summary>
    public class PrinterManager : AbstractModule
    {
        public PrinterManager()
        {
            Name = "PrinterManager";
        }

        protected override void DoWork()
        {
            //Get printers
            var printerResponse = Communication.GetResponse("/service/Printers.php", true);
            if (printerResponse.Error || printerResponse.GetField("#mode").Equals("0")) return;

            Log.Entry(Name, "Creating list of printers");
            var printerIDs = printerResponse.GetList("#printer", false);
            Log.Entry(Name, "Creating printer objects");
            var printers = CreatePrinters(printerIDs);

            if (printerResponse.GetField("#mode").Equals("ar"))
                RemoveExtraPrinters(printers);

            Log.Entry(Name, "Adding printers");
            foreach (var printer in printers)
            {
                printer.Add();
                if(printer.Default)
                    printer.SetDefault();
            }
        }

        private static void RemoveExtraPrinters(List<Printer> newPrinters)
        {
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");

            foreach (var name in from ManagementBaseObject printer in printerQuery.Get()
                    select printer.GetPropertyValue("Name").ToString()
                    into name
                    let safe = newPrinters.Any(newPrinter => newPrinter.Name.Equals(name))
                    where !safe
                    select name)
                Printer.Remove(name);
        }

        private List<Printer> CreatePrinters(List<string> printerIDs)
        {
            try
            {
                return (from id in printerIDs 
                        select Communication.GetResponse(string.Format("/service/Printers.php?id={0}", id), true) 
                            into printerData 
                        where !PrinterExists(printerData)
                        select PrinterFactory(printerData)).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
                return new List<Printer>();
            }

        }

        private static bool PrinterExists(Response printerData )
        {
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            if (
                (from ManagementBaseObject printer in printerQuery.Get() select printer.GetPropertyValue("Name"))
                    .Contains(printerData.GetField("#name")))
                return true;

            return false;
        }

        private static Printer PrinterFactory(Response printerData)
        {
            if (PrinterExists(printerData)) return null;

            if(printerData.GetField("#type").Equals("iPrint"))
                return new iPrintPrinter(printerData.GetField("#name"), 
                    printerData.GetField("#ip"), 
                    printerData.GetField("#port"),
                    printerData.GetField("#default").Equals("1"));

            if (printerData.GetField("#type").Equals("Network"))
                return new NetworkPrinter(printerData.GetField("#name"),
                    printerData.GetField("#ip"),
                    printerData.GetField("#port"),
                    printerData.GetField("#default").Equals("1"));

            if (printerData.GetField("#type").Equals("Local"))
                return new LocalPrinter(printerData.GetField("#name"),
                    printerData.GetField("#file"),
                    printerData.GetField("#port"),
                    printerData.GetField("#ip"),
                    printerData.GetField("#model"),
                    printerData.GetField("#default").Equals("1"));

            return null;
        }
    }
}
