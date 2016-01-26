﻿/*
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
using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;

// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace FOG.Modules.PrinterManager
{
    /// <summary>
    ///     Manage printers
    /// </summary>
    public class PrinterManager : AbstractModule
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

        protected override void DoWork()
        {
            //Get printers
            var printerResponse = Communication.GetResponse("/service/Printers.php", true);

            if (printerResponse.GetField("#mode").Equals("0")) return;

            if (printerResponse.Error && printerResponse.ReturnCode.Equals("#!np"))
            {
                RemoveExtraPrinters(new List<Printer>(), printerResponse.GetField("#mode").Equals("ar"));
                return;
            }
            if (printerResponse.Error) return;
            if (!printerResponse.Encrypted)
            {
                Log.Error(Name, "Response was not encrypted");
                return;
            }

            Log.Entry(Name, "Creating list of printers");
            var printerIDs = printerResponse.GetList("#printer", false);
            Log.Entry(Name, "Creating printer objects");
            var printers = CreatePrinters(printerIDs);

            RemoveExtraPrinters(printers, printerResponse.GetField("#mode").Equals("ar"));

            Log.Entry(Name, "Adding printers");
            foreach (var printer in printers)
            {
                if (!PrinterExists(printer.Name))
                    printer.Add(_instance);
                else
                    Log.Entry(Name, printer.Name + " already exists");
            }
        }

        private void RemoveExtraPrinters(List<Printer> newPrinters, bool removeAll = false)
        {
            Log.Debug(Name, "Removing extra printers...");


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
                return _instance.GetPrinters().Contains(name);
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not detect if printer exists");
                Log.Error(Name, ex);
            }

            return false;
        }

        public static List<Printer> CreatePrinters(List<string> printerIDs)
        {
            try
            {
                return
                    printerIDs.Select(
                        id => Communication.GetResponse($"/service/Printers.php?id={id}", true))
                        .Select(PrinterFactory)
                        .Where(printer => printer != null)
                        .ToList();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, ex);
                return new List<Printer>();
            }
        }

        public static Printer PrinterFactory(Response printerData)
        {
            if (printerData.GetField("#type").Equals("iPrint"))
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
                    printerData.GetField("#default").Equals("1"),
                    printerData.GetField("#configFile"));                  

            return null;
        }
    }
}