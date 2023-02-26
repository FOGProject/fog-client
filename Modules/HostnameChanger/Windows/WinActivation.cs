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
using System.Linq;
using System.Runtime.InteropServices;
using Zazzles;
using SLID = System.Guid;

namespace FOG.Modules.HostnameChanger.Windows
{
    class WinActivation
    {
        private static string LogName = "WinActivation";
        private enum SL_GENUINE_STATE
        {
            SL_GEN_STATE_IS_GENUINE = 0,
            SL_GEN_STATE_INVALID_LICENSE = 1,
            SL_GEN_STATE_TAMPERED = 2,
            SL_GEN_STATE_LAST = 3
        }

        //https://theroadtodelphi.wordpress.com/2009/10/12/determine-genuine-windows-installation-in-c/
        [DllImport("Slwga.dll", EntryPoint = "SLIsGenuineLocal", CharSet = CharSet.None, ExactSpelling = false,
            SetLastError = false, PreserveSig = true, CallingConvention = CallingConvention.Winapi,
            BestFitMapping = false, ThrowOnUnmappableChar = false)]
        [PreserveSigAttribute()]
        private static extern uint SLIsGenuineLocal(ref SLID slid, [In, Out] ref SL_GENUINE_STATE genuineState, IntPtr val3);

        public static bool IsActivated()
        {
            var windowsGUID = new Guid("55c92734-d682-4d71-983e-d6ec3f16059f");
            SLID windowsSLID = (Guid)windowsGUID;

            try
            {
                var genuineState = SL_GENUINE_STATE.SL_GEN_STATE_LAST;
                var result = SLIsGenuineLocal(ref windowsSLID, ref genuineState, IntPtr.Zero);
                if (result == 0)
                    return genuineState == SL_GENUINE_STATE.SL_GEN_STATE_IS_GENUINE;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to check activation state");
                Log.Error(LogName, ex);
            }

            return false;
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
            var exitCode = RunSLMGR("/ato");

            if (exitCode == -1073430520)
            {
                Log.Error(LogName, "Windows rejected product key");
            }

            return exitCode == 0;
        }

        private static int RunSLMGR(params string[] args)
        {
            var slmgrLoc = Path.Combine(Environment.SystemDirectory, "slmgr.vbs");
            var procArg = $@"/B /Nologo {slmgrLoc} {string.Join(" ", args)}";

            return ProcessHandler.Run("cscript", procArg);
        }

        private static string[] GetSLMGROutput(params string[] args)
        {
            var slmgrLoc = Path.Combine(Environment.SystemDirectory, "slmgr.vbs");
            var procArg = $@"{slmgrLoc} {string.Join(" ", args)}";

            string[] stdout;
            ProcessHandler.Run("cscript", procArg, true, out stdout);
            return stdout;
        }
    }
}