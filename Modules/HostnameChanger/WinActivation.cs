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
using System.Linq;
using FOG.Handlers;

namespace FOG.Modules.HostnameChanger
{
    class WinActivation
    {
        private static string LogName = "WinActivation";

        public static bool IsActivated()
        {
            var info = GetSLMGROutput("/dli");
            var flattenedInfo = string.Join(" ", info);

            return flattenedInfo.Contains("Licensed");
        }

        public static string GetPartialKey()
        {
            var info = GetSLMGROutput("/dli");
            var key = "";

            foreach (var infoLine in info.Where(infoLine => infoLine.Trim().StartsWith("Partial Product Key")))
            {
                key = infoLine.Substring(infoLine.IndexOf(":") + 1).Trim();
                break;
            }

            return key;
        }

        public static bool SetProductKey(string key)
        {
            return InstallProductKey(key) && ActivateInstalledKey();
        }

        private static bool InstallProductKey(string key)
        {
            Log.Entry(LogName, "Installing Product key");
            return RunSLMGR("/ipk", key) == 0;
        }

        private static bool ActivateInstalledKey()
        {
            Log.Entry(LogName, "Activating Product key");
            return RunSLMGR("/ato") == 0;
        }

        private static int RunSLMGR(params string[] args)
        {
            var slmgrLoc = Path.Combine(Environment.SystemDirectory, "slmgr.vbs");
            var procArg = $@"/B /Nologo {slmgrLoc} {string.Join(" ", args)}";
        
            return ProcessHandler.Run("cscript", true, procArg);
        }

        private static string[] GetSLMGROutput(params string[] args)
        {
            var slmgrLoc = Path.Combine(Environment.SystemDirectory, "slmgr.vbs");
            var procArg = $@"/B /Nologo {slmgrLoc} {string.Join(" ", args)}";

            return ProcessHandler.GetOutput("cscript", procArg);
        }
    }
}
