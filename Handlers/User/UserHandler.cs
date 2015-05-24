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

namespace FOG.Handlers
{
    /// <summary>
    ///     Detect the current user
    /// </summary>
    public static class UserHandler
    {
        private const string LogName = "UserHandler";

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref Lastinputinfo plii);

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass,
            out IntPtr ppBuffer, out int pBytesReturned);

        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);


        private enum WtsInfoClass
        {
            WTSUserName,
            WTSDomainName
        };

        internal struct Lastinputinfo
        {
            public uint CbSize;
            public uint DwTime;
        }

        /// <summary>
        /// </summary>
        /// <returns>True if a user is logged in</returns>
        public static bool IsUserLoggedIn()
        {
            return GetUsersLoggedIn().Count > 0;
        }

        /// <summary>
        /// </summary>
        /// <returns>The current username</returns>
        public static string GetCurrentUser()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();

            return (windowsIdentity == null)
                ? null
                : windowsIdentity.Name;
        }

        /// <summary>
        /// </summary>
        /// <returns>A list of all users and their security IDs</returns>
        public static List<UserData> GetAllUserData()
        {
            var query = new SelectQuery("Win32_UserAccount");
            var searcher = new ManagementObjectSearcher(query);

            return (from ManagementBaseObject envVar in searcher.Get() select new UserData(envVar["Name"].ToString(), 
                envVar["SID"].ToString())).ToList();
        }

        /// <summary>
        /// </summary>
        /// <returns>The inactivity time of the current user in seconds</returns>
        public static int GetInactivityTime()
        {
            var lastInputInfo = new Lastinputinfo();
            lastInputInfo.CbSize = (uint) Marshal.SizeOf(lastInputInfo);
            lastInputInfo.DwTime = 0;

            var envTicks = (uint) Environment.TickCount;

            if (!GetLastInputInfo(ref lastInputInfo))
                return 0;
                
            var lastInputTick = lastInputInfo.DwTime;
            var idleTime = envTicks - lastInputTick;

            return (int) idleTime/1000;
        }

        /// <summary>
        ///     Get a list of usernames logged in
        /// </summary>
        /// <returns>A list of usernames</returns>
        public static List<string> GetUsersLoggedIn()
        {
            var sessionIds = GetSessionIds();

            return (from sessionId in sessionIds where !GetUserNameFromSessionId(sessionId, false)
                        .Equals("SYSTEM") select GetUserNameFromSessionId(sessionId, false)).ToList();
        }

        /// <summary>
        ///     Get all active session IDs
        /// </summary>
        /// <returns>A list of session IDs</returns>
        public static List<int> GetSessionIds()
        {
            var sessionIds = new List<int>();
            var properties = new[] {"SessionId"};

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
                    LogHandler.Error(LogName, "Unable to parse Session ID");
                    LogHandler.Error(LogName, ex);
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
        public static string GetUserNameFromSessionId(int sessionId, bool prependDomain)
        {
            IntPtr buffer;
            int strLen;
            var username = "SYSTEM";
            if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) || strLen <= 1)
                return username;

            username = Marshal.PtrToStringAnsi(buffer);
            WTSFreeMemory(buffer);

            if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) || strLen <= 1)
                return username;
   
            if(prependDomain)
                username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;


            WTSFreeMemory(buffer);
            return username;
        }

        /// <summary>
        /// </summary>
        /// <param name="sid">The user's security ID</param>
        /// <returns>The user's profile path</returns>
        public static string GetProfilePath(string sid)
        {
            return RegistryHandler.GetRegisitryValue(string.Format(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\{0}\", sid), "ProfileImagePath");
        }
    }
}