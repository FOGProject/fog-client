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

using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;

// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace FOG.Modules.PrinterManager
{
    /// <summary>
    ///     Manage printers
    /// </summary>
    public class DefaultPrinterManager : AbstractModule<DefaultPrinterMessage>
    {
        private readonly PrintManagerBridge _instance;

        public DefaultPrinterManager()
        {
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

        protected override void DoWork(Response data, DefaultPrinterMessage msg)
        {
            //Get printers
            if (data.Error || string.IsNullOrWhiteSpace(msg.Name)) return;

            Log.Entry(Name, "Checking defaults");
            var printer = new Printer {Name = msg.Name};
            printer.SetDefault(_instance);
        }
    }
}