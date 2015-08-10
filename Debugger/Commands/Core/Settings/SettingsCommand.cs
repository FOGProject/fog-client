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

using FOG.Handlers;

namespace FOG.Commands.Core.Settings
{
    internal class SettingsCommand : ICommand
    {
        private const string LogName = "Console::Settings";

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }


            if (args[0].Equals("os"))
            {
                Log.WriteLine("--> " + Handlers.Settings.OS);
                return true;
            }

            if (args[0].Equals("reload"))
            {
                Handlers.Settings.Reload();
                Log.WriteLine("--> " + "Reloaded");

                return true;
            }

            if (args.Length < 1) return false;

            if (args[0].Equals("get"))
            {
                Log.WriteLine("--> " + args[1] + " = \"" + Handlers.Settings.Get(args[0]) + "\"");
                return true;
            }

            if (args[0].Equals("path"))
            {
                Handlers.Settings.SetPath(args[1]);
                Log.WriteLine("--> " + "Complete");
                return true;
            }

            if (args.Length < 2) return false;

            if (args[0].Equals("set"))
            {
                Handlers.Settings.Set(args[1], args[2]);
                Log.WriteLine("--> " + "Complete");
                return true;
            }

            return false;
        }

        private static void Help()
        {
            Log.WriteLine("Avaible commands");
            Log.WriteLine("--> os");
            Log.WriteLine("--> get [SETTING]");
            Log.WriteLine("--> set [SETTING] [VALUE]");
            Log.WriteLine("--> path [PATH]");
            Log.WriteLine("--> reload");
        }
    }
}