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
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using ProcessPrivileges;

namespace FOG.Handlers
{
    /// <summary>
    ///     Detect the current user
    /// </summary>
    public static class UserHandler
    {
        private const string LOG_NAME = "UserHandler";

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass,
            out IntPtr ppBuffer, out int pBytesReturned);

        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);

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
            return WindowsIdentity.GetCurrent().Name;
        }

        /// <summary>
        /// </summary>
        /// <returns>A list of all users and their security IDs</returns>
        public static List<UserData> GetAllUserData()
        {
            var users = new List<UserData>();

            var query = new SelectQuery("Win32_UserAccount");
            var searcher = new ManagementObjectSearcher(query);

            foreach (var envVar in searcher.Get())
            {
                var userData = new UserData(envVar["Name"].ToString(), envVar["SID"].ToString());
                users.Add(userData);
            }

            return users;
        }

        /// <summary>
        /// </summary>
        /// <returns>The inactivity time of the current user in seconds</returns>
        public static int GetUserInactivityTime()
        {
            uint idleTime = 0;
            var lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint) Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            var envTicks = (uint) Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                var lastInputTick = lastInputInfo.dwTime;

                idleTime = envTicks - lastInputTick;
            }

            return (int) idleTime/1000;
        }

        /// <summary>
        ///     Get a list of usernames logged in
        /// </summary>
        /// <returns>A list of usernames</returns>
        public static List<string> GetUsersLoggedIn()
        {
            var users = new List<string>();
            var sessionIds = GetSessionIds();

            foreach (var sessionId in sessionIds)
            {
                if (!GetUserNameFromSessionId(sessionId, false).Equals("SYSTEM"))
                    users.Add(GetUserNameFromSessionId(sessionId, false));
            }

            return users;
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
                    {
                        sessionIds.Add(int.Parse(envVar["SessionId"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.Log(LOG_NAME, "Unable to parse Session Id");
                    LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
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
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) &&
                strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
                if (prependDomain)
                {
                    if (
                        WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer,
                            out strLen) && strLen > 1)
                    {
                        username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
                        WTSFreeMemory(buffer);
                    }
                }
            }
            return username;
        }

        /// <summary>
        /// </summary>
        /// <param name="sid">The user's security ID</param>
        /// <returns>The user's profile path</returns>
        public static string GetUserProfilePath(string sid)
        {
            return
                RegistryHandler.GetRegisitryValue(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + sid + @"\", "ProfileImagePath");
        }

        /// <summary>
        ///     Completely remove a user from the registry and the filesystem
        /// </summary>
        /// <param name="user">The user to purge</param>
        /// <param name="deleteData">If the user profile should be deleted</param>
        /// <returns>True if sucessfull</returns>
        public static bool PurgeUser(UserData user, bool deleteData)
        {
            LogHandler.Log(LOG_NAME, "Purging " + user.Name + " from system");
            if (deleteData)
            {
                if (UnregisterUser(user.Name))
                {
                    if (RemoveUserProfile(user.SID))
                    {
                        return CleanUserRegistryEntries(user.SID);
                    }
                }
                return false;
            }
            return UnregisterUser(user.Name);
        }

        /// <summary>
        ///     Unregister a user from windows
        /// </summary>
        /// <param name="user">The username to unregister</param>
        /// <returns>True is sucessfull</returns>
        public static bool UnregisterUser(string user)
        {
            try
            {
                var userDir = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                var userToDelete = userDir.Children.Find(user);

                userDir.Children.Remove(userToDelete);
                return true;
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Unable to unregister user");
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        ///     Attempt to delete the user profile, current not working
        /// </summary>
        /// <param name="sid">The user's security ID</param>
        /// <returns>True if sucessfull</returns>
        public static bool RemoveUserProfile(string sid)
        {
            try
            {
                var path = GetUserProfilePath(sid);
                LogHandler.Log(LOG_NAME, "User path: " + path);
                if (path != null)
                {
                    TakeOwnership(path);
                    resetRights(path);
                    RemoveWriteProtection(path);
                    Directory.Delete(path, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Unable to remove user data");
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        ///     Remove a user from the registry
        /// </summary>
        /// <param name="sid">The user's security ID</param>
        /// <returns>True if sucessfull</returns>
        public static bool CleanUserRegistryEntries(string sid)
        {
            return
                RegistryHandler.DeleteFolder(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + sid + @"\");
        }

        /// <summary>
        ///     Take ownership of a directory
        /// </summary>
        /// <param name="path">The directory path</param>
        public static void TakeOwnership(string path)
        {
            using (new PrivilegeEnabler(Process.GetCurrentProcess(), Privilege.TakeOwnership))
            {
                var directoryInfo = new DirectoryInfo(path);
                var directorySecurity = directoryInfo.GetAccessControl();
                directorySecurity.SetOwner(WindowsIdentity.GetCurrent().User);
                Directory.SetAccessControl(path, directorySecurity);
            }
        }

        private static DirectorySecurity removeExplicitSecurity(DirectorySecurity directorySecurity)
        {
            var rules = directorySecurity.GetAccessRules(true, false, typeof (NTAccount));
            foreach (FileSystemAccessRule rule in rules)
                directorySecurity.RemoveAccessRule(rule);
            return directorySecurity;
        }

        /// <summary>
        ///     Reset the rights of a directory
        /// </summary>
        /// <param name="path">The directory path</param>
        public static void resetRights(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var directorySecurity = directoryInfo.GetAccessControl();
            directorySecurity = removeExplicitSecurity(directorySecurity);
            Directory.SetAccessControl(path, directorySecurity);
        }

        /// <summary>
        ///     Remove write protection on a directory
        /// </summary>
        /// <param name="path">The directory path</param>
        public static void RemoveWriteProtection(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
        }

        private enum WtsInfoClass
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
            WTSSessionInfo
        }
            ;

        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
    }
}