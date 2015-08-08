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
using FOG.Handlers;

namespace FOG.Modules.PrinterManager
{
    public abstract class PrintManagerBridge
    {
        public abstract List<string> GetPrinters();

        public void Add(Printer printer)
        {
            try
            {
                if (printer is iPrintPrinter)
                    AddiPrint((iPrintPrinter)printer);
                else if (printer is LocalPrinter)
                    AddLocal((LocalPrinter)printer);
                else if (printer is NetworkPrinter)
                    AddNetwork((NetworkPrinter)printer);
            }
            catch (Exception ex)
            {
                Log.Error("Printer", "Could not add");
                Log.Error("Printer", ex);
            }
        }

        protected abstract void AddiPrint(iPrintPrinter printer);
        protected abstract void AddLocal(LocalPrinter printer);
        protected abstract void AddNetwork(NetworkPrinter printer);

        public abstract void Remove(string name);
        public abstract void Default(string name);
    }
}
