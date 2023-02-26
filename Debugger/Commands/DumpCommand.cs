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
using System.IO;
using Zazzles;
using Zazzles.Debugger.Commands;
using Zazzles.Middleware;

namespace FOG.Commands
{
    internal class DumpCommand : ICommand
    {
        private const string LogName = "Console::Dump";

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }
            else if (args[0].Equals("cycle", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var rawResponse = Communication.GetRawResponse("/management/index.php?sub=requestClientInfo&mac=" + Configuration.MACAddresses());

                    var encrypted = rawResponse.StartsWith("#!en");
                    if (encrypted)
                        rawResponse = Authentication.Decrypt(rawResponse);

                    Log.Entry(LogName, "Dumping decrypted response");
                    Log.Divider();
                    Log.WriteLine(rawResponse);
                    Log.Divider();

                    if (args.Length > 1 && args[1].Equals("save", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = Path.Combine(Settings.Location, "FOGCycle.txt");
                        Log.WriteLine("Saving output to " + path);

                        File.WriteAllText(path, rawResponse);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, "Unable to get cycle data");
                    Log.Error(LogName, ex);
                }
                return true;
            }

            return true;
        }

        private void Help()
        {
            Log.WriteLine("Available commands");
            Log.WriteLine("--> cycle");
            Log.WriteLine("--> cycle save");
        }
    }
}