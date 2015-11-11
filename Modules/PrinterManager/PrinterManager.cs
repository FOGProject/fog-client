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
        private static string LogName;

        public PrinterManager()
        {
            Name = "PrinterManager";
            LogName = Name;
        }

        protected override void DoWork()
        {
            //Get printers
            var printerResponse = Communication.GetResponse("/service/Printers.php", true);

            if (!printerResponse.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            if (printerResponse.GetField("#mode").Equals("0")) return;

            if (printerResponse.Error && printerResponse.ReturnCode.Equals("#!np"))
            {
                RemoveExtraPrinters(new List<Printer>(), printerResponse.GetField("#mode").Equals("ar"));
                return;
            }
            if (printerResponse.Error) return;

            Log.Entry(Name, "Creating list of printers");
            var printerIDs = printerResponse.GetList("#printer", false);
            Log.Entry(Name, "Creating printer objects");
            var printers = CreatePrinters(printerIDs);

            RemoveExtraPrinters(printers, printerResponse.GetField("#mode").Equals("ar"));

            Log.Entry(Name, "Adding printers");
            foreach (var printer in printers)
            {
                if(!PrinterExists(printer.Name))
                    printer.Add();
                else
                    Log.Entry(Name, printer.Name + " already exists");
            }
        }

        private void RemoveExtraPrinters(List<Printer> newPrinters, bool removeAll = false)
        {
            Log.Debug(Name, "Removing extra printers...");

            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");

            Log.Debug(Name, "Stripping printer data");

            var managedPrinters = new List<string>();
            foreach (var printer in newPrinters.Where(printer => printer != null))
            {
                Log.Debug(Name, "Stripping " + printer.Name);
                managedPrinters.Add(printer.Name);
            }

            if (!removeAll)
            {

                var allPrinters = Communication.GetResponse("/service/printerlisting.php");

                if (allPrinters.Error) return;
                var printerNames = allPrinters.GetList("#printer", false);
                foreach (var name in printerNames.Where(name => !managedPrinters.Contains(name) && PrinterExists(name)))
                    Printer.Remove(name);
            }
            else
            {
                foreach (var printer in printerQuery.Get().Cast<ManagementBaseObject>().Where(printer => !managedPrinters.Contains(printer.GetPropertyValue("Name").ToString().Trim())))
                {
                    Printer.Remove(printer.GetPropertyValue("Name").ToString());
                }
            }
        }

        public static List<Printer> CreatePrinters(List<string> printerIDs)
        {
            try
            {
                return printerIDs.Select(id => Communication.GetResponse(string.Format("/service/Printers.php?id={0}", id), true)).Select(PrinterFactory).Where(printer => printer != null).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, ex);
                return new List<Printer>();
            }

        }

        public static bool PrinterExists(string name)
        {
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            return (from ManagementBaseObject printer in printerQuery.Get() select printer.GetPropertyValue("Name"))
                .Contains(name);
        }

        public static Printer PrinterFactory(Response printerData)
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
