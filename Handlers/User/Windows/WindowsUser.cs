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
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace FOG.Handlers.User
{
    class WindowsUser : IUser
    {
        private const string LogName = "UserHandler";
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref Lastinputinfo plii);

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out System.IntPtr ppBuffer, out int pBytesReturned);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);

        public enum WtsInfoClass
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo,
        }

        internal struct Lastinputinfo
        {
            public uint CbSize;
            public uint DwTime;
        }

        /// <summary>
        /// </summary>
        /// <returns>The inactivity time of the current user in seconds</returns>
        public int GetInactivityTime()
        {
            var lastInputInfo = new Lastinputinfo();
            lastInputInfo.CbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.DwTime = 0;

            var envTicks = (uint)Environment.TickCount;

            if (!GetLastInputInfo(ref lastInputInfo))
                return 0;

            var lastInputTick = lastInputInfo.DwTime;
            var idleTime = envTicks - lastInputTick;

            return (int)idleTime / 1000;
        }

        /// <summary>
        ///     Get a list of usernames logged in
        /// </summary>
        /// <returns>A list of usernames</returns>
        public List<string> GetUsersLoggedIn()
        {
            var sessionIds = GetSessionIds();

            return (from sessionId in sessionIds
                    where !GetUserNameFromSessionId(sessionId, false)
                        .Equals("SYSTEM")
                    select GetUserNameFromSessionId(sessionId, false)).ToList();
        }

        /// <summary>
        ///     Get all active session IDs
        /// </summary>
        /// <returns>A list of session IDs</returns>
        private static List<int> GetSessionIds()
        {
            var sessionIds = new List<int>();
            var properties = new[] { "SessionId" };

            var query = new SelectQuery("Win32_Process", "", properties); //SessionId
            var searcher = new ManagementObjectSearcher(query);

            foreach (var envVar in searcher.Get())
            {
                try
                {
                    if (!sessionIds.Contains(int.Parse(envVar["SessionId"].ToString())))
                        sessionIds.Add(int.Parse(envVar["SessionId"].ToString()));
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, "Unable to parse Session ID");
                    Log.Error(LogName, ex);
                }
            }

            return sessionIds;
        }

        /// <summary>
        ///     Convert a session ID to its correlating username
        /// </summary>
        /// <param name="sessionId">The session ID to use</param>
        /// <param name="prependDomain">If the user's domain should be prepended</param>
        /// <returns>The username</returns>
        //https://stackoverflow.com/questions/19487541/get-windows-user-name-from-sessionid
        private static string GetUserNameFromSessionId(int sessionId, bool prependDomain)
        {
            IntPtr buffer;
            int strLen;
            var username = "SYSTEM";
            if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) ||
                strLen <= 1) return username;
            username = Marshal.PtrToStringAnsi(buffer);
            WTSFreeMemory(buffer);
            if (!prependDomain) return username;
            if (
                !WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) ||
                strLen <= 1) return username;
            username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
            WTSFreeMemory(buffer);
            return username;
        }
    }
}
