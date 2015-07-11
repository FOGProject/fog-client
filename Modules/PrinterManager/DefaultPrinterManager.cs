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
    public class DefaultPrinterManager : AbstractModule
    {
        public DefaultPrinterManager()
        {
            Name = "DefaultPrinterManager";
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

            Log.Entry(Name, "Checking defaults");
            foreach (var printer in printers.Where(printer => printer.Default))
            {
                printer.SetDefault();
            }
        }

        private List<Printer> CreatePrinters(List<string> printerIDs)
        {
            try
            {
                return printerIDs.Select(id => Communication.GetResponse(string.Format("/service/Printers.php?id={0}", id), true)).Select(PrinterFactory).Where(printer => printer != null).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
                return new List<Printer>();
            }

        }

        private static Printer PrinterFactory(Response printerData)
        {
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
