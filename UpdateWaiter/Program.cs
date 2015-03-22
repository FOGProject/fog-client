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
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace FOG
{
    class Program
    {
		
        public static void Main(string[] args)
        {
            //Update Line
            //Check if an parameter was passed
            if (args.Length > 0)
            {
                //Wait for all update files to be applied
                while (updateFilePresent())
                {
                }				
                //Spawn the process that originally called this program
                if (File.Exists(args[0]))
                    spawnParentProgram(args[0]);
            }
			
        }
		
        private static Boolean updateFilePresent()
        {
            Boolean fileFound = false;
            foreach (String fileName in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                if (fileName.EndsWith("updating.info"))
                    fileFound = true;
            }
            Thread.Sleep(10 * 1000);
			
            return fileFound;
        }
		
        private static void spawnParentProgram(String fileName)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = fileName;
            process.Start();				
        }
		
    }
}