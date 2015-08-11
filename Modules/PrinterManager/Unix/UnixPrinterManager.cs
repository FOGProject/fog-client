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

namespace FOG.Modules.PrinterManager
{
    internal class UnixPrinterManager : PrintManagerBridge
    {
        public override List<string> GetPrinters()
        {
            var output = ProcessHandler.GetOutput("lpstat",  "-p | awk '{ print $2}' ");
            return new List<string>(output);
        }

        protected override void AddiPrint(iPrintPrinter printer)
        {
            throw new NotImplementedException();
        }

        protected override void AddLocal(LocalPrinter printer)
        {
            throw new NotImplementedException();
        }

        protected override void AddNetwork(NetworkPrinter printer)
        {
            throw new NotImplementedException();
        }

        protected override void AddCUPS(CUPSPrinter printer)
        {
            var portName = ProcessHandler.GetOutput("echo", string.Format("{0} {1}",printer.Name," | tr ' ' '_'"));
            var lpdAddress = string.Format("{0} {1}","lpd://",printer.IP);
            ProcessHandler.Run("lpadmin", string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}","-p",portName, "-E","-v",lpdAddress,"-P",printer.File, "-D",printer.Name));
        }

        public override void Remove(string name)
        {
            ProcessHandler.Run("lpstat", string.Format("{0} {1} {2}", "-","-P",name));
        }

        public override void Default(string name)
        {
            throw new NotImplementedException();
        }
    }
}