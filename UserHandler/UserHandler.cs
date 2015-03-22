
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Management;
using System.DirectoryServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;

namespace FOG
{
    /// <summary>
    /// Detect the current user
    /// </summary>
    public static class UserHandler
    {
		
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
		
        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out System.IntPtr ppBuffer, out int pBytesReturned);
	    
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);

        enum WtsInfoClass
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
            WTSSessionInfo}

        ;
    
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
		
        private const String LOG_NAME = "UserHandler";

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if a user is logged in</returns>
        public static Boolean IsUserLoggedIn()
        {
            return GetUsersLoggedIn().Count > 0;
        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <returns>The current username</returns>
        public static String GetCurrentUser()
        {
            return WindowsIdentity.GetCurrent().Name;
        }

		
        /// <summary>
        /// 
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
        ///  
        /// </summary>
        /// <returns>The inactivity time of the current user in seconds</returns>
        public static int GetUserInactivityTime()
        {
            uint idleTime = 0;
            var lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;
	
            uint envTicks = (uint)Environment.TickCount;
	
            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;
	
                idleTime = envTicks - lastInputTick;
            }
			
            return (int)idleTime / 1000;
        }
		
        /// <summary>
        /// Get a list of usernames logged in
        /// </summary>
        /// <returns>A list of usernames</returns>
        public static List<String> GetUsersLoggedIn()
        {
            var users = new List<String>();
            var sessionIds = GetSessionIds();
			
            foreach (int sessionId in sessionIds)
            {
                if (!GetUserNameFromSessionId(sessionId, false).Equals("SYSTEM"))
                    users.Add(GetUserNameFromSessionId(sessionId, false));
            }
			
            return users;
        }
		
        /// <summary>
        /// Get all active session IDs
        /// </summary>
        /// <returns>A list of session IDs</returns>
        public static List<int> GetSessionIds()
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
        /// Convert a session ID to its correlating username
        /// </summary>
        /// <param name="sessionId">The session ID to use</param>
        /// <param name="prependDomain">If the user's domain should be prepended</param>
        /// <returns>The username</returns>
        //https://stackoverflow.com/questions/19487541/get-windows-user-name-from-sessionid
        public static String GetUserNameFromSessionId(int sessionId, bool prependDomain)
        {
            IntPtr buffer;
            int strLen;
            string username = "SYSTEM";
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
                if (prependDomain)
                {
                    if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) && strLen > 1)
                    {
                        username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
                        WTSFreeMemory(buffer);
                    }
                }
            }
            return username;
        }
		
        /// <summary>
        ///
        /// </summary>
        /// <param name="sid">The user's security ID</param>
        /// <returns>The user's profile path</returns>
        public static String GetUserProfilePath(String sid)
        {
            return RegistryHandler.GetRegisitryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + sid + @"\", "ProfileImagePath");
        }
		
        /// <summary>
        /// Completely remove a user from the registry and the filesystem
        /// </summary>
        /// <param name="user">The user to purge</param>
        /// <param name="deleteData">If the user profile should be deleted</param>
        /// <returns>True if sucessfull</returns>
        public static Boolean PurgeUser(UserData user, Boolean deleteData)
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
            else
            {
                return UnregisterUser(user.Name);
            }
        }
	
        /// <summary>
        /// Unregister a user from windows
        /// </summary>
        /// <param name="user">The username to unregister</param>
        /// <returns>True is sucessfull</returns>
        public static Boolean UnregisterUser(String user)
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
        /// Attempt to delete the user profile, current not working
        /// </summary>
        /// <param name="sid">The user's security ID</param>
        /// <returns>True if sucessfull</returns>
        public static Boolean RemoveUserProfile(String sid)
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
        /// Remove a user from the registry
        /// </summary>
        /// <param name="sid">The user's security ID</param>
        /// <returns>True if sucessfull</returns>
        public static Boolean CleanUserRegistryEntries(String sid)
        {
            return RegistryHandler.DeleteFolder(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + sid + @"\");
        }
		
        /// <summary>
        /// Take ownership of a directory
        /// </summary>
        /// <param name="path">The directory path</param>
        public static void TakeOwnership(String path)
        {

            using (new ProcessPrivileges.PrivilegeEnabler(Process.GetCurrentProcess(), ProcessPrivileges.Privilege.TakeOwnership))
            {
                var directoryInfo = new DirectoryInfo(path);
                DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
                directorySecurity.SetOwner(WindowsIdentity.GetCurrent().User);
                Directory.SetAccessControl(path, directorySecurity);    
            }

        }
		
        private static DirectorySecurity removeExplicitSecurity(DirectorySecurity directorySecurity)
        {
            var rules = directorySecurity.GetAccessRules(true, false, typeof(NTAccount));
            foreach (FileSystemAccessRule rule in rules)
                directorySecurity.RemoveAccessRule(rule);
            return directorySecurity;
        }
		
        /// <summary>
        /// Reset the rights of a directory
        /// </summary>
        /// <param name="path">The directory path</param>
        public static void resetRights(String path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var directorySecurity = directoryInfo.GetAccessControl();
            directorySecurity = removeExplicitSecurity(directorySecurity);
            Directory.SetAccessControl(path, directorySecurity);
        }
		
        /// <summary>
        /// Remove write protection on a directory
        /// </summary>
        /// <param name="path">The directory path</param>
        public static void RemoveWriteProtection(String path)
        {
            var directoryInfo = new DirectoryInfo(path);
            directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
        }
		

    }
}