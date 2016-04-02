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

namespace FOG.Modules.PrinterManager
{
    public class LocalPrinter : Printer
    {
        public LocalPrinter(string name, string file, string configFile, string port, string ip, string model, bool defaulted)
        {
            Name = name;
            Port = port;
            File = file;
            ConfigFile = configFile;
            Model = model;
            Default = defaulted;
            IP = ip;
            LogName = "LocalPrinter";
        }

        public override void Add(PrintManagerBridge instance)
        {
            instance.Add(this);
        }
    }
}