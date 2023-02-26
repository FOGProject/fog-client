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
    public class UnixPrinterManager : PrintManagerBridge
    {
        public override List<string> GetPrinters()
        {
            string[] stdout;
            ProcessHandler.Run("lpstat",  "-p | awk '{ print $2}' ", true, out stdout);
            return new List<string>(stdout);
        }

        protected override void AddiPrint(Printer printer, bool verbose = false)
        {
            throw new NotImplementedException();
        }

        protected override void AddLocal(Printer printer, bool verbose = false)
        {
            throw new NotImplementedException();
        }

        protected override void AddNetwork(Printer printer, bool verbose = false)
        {
            throw new NotImplementedException();
        }

        protected override void AddCUPS(Printer printer, bool verbose = false)
        {
            string[] stdout;
            ProcessHandler.Run("echo", $"{printer.Name} | tr ' ' '_'", true, out stdout);
            var portName = stdout[0];

            var lpdAddress = $"lpd://{printer.IP}";
            ProcessHandler.Run("lpadmin",
                $"-p {portName} -E -v {lpdAddress} -P {printer.File} -D {printer.Name}");
        }

        public override void Remove(string name, bool verbose = false)
        {
            ProcessHandler.Run("lpstat", $"- -P {name}");
        }

        public override void Default(string name, bool verbose = false)
        {
            throw new NotImplementedException();
        }

        public override void Configure(Printer printer, bool verbose = false)
        {
            throw new NotImplementedException();
        }

        public override void ApplyChanges()
        {
            
        }
    }
}