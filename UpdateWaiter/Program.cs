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

using Zazzles;
using Zazzles.Modules.Updater;

namespace FOG
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Eager.Initalize();

            //Update Line
            //Check if an parameter was passed
            if (args.Length == 0) return;

            //Wait for all update files to be applied
            while (UpdaterHelper.Updating()) { }
            //Spawn the process that originally called this program
            var param = "";
            if (args.Length > 1)
                param = args[1];

            SpawnParentProgram(args[0], param);
        }

        private static void SpawnParentProgram(string fileName, string param)
        {
            ProcessHandler.RunEXE(fileName, param, false);
        }
    }
}