
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Management;
using System.DirectoryServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;

namespace FOG {
	/// <summary>
	/// Detect the current user
	/// </summary>
	public static class UserHandler {
		
		[DllImport("user32.dll")]
		static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
		
		[DllImport("Wtsapi32.dll")]
	    private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out System.IntPtr ppBuffer, out int pBytesReturned);
	    
	    [DllImport("Wtsapi32.dll")]
	    private static extern void WTSFreeMemory(IntPtr pointer);

		enum WtsInfoClass {
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
		};
    
		internal struct LASTINPUTINFO {
			public uint cbSize;
			public uint dwTime;
		}
		
		private const String LOG_NAME = "UserHandler";

		//Check if a user is loggin in, do this by getting a list of all users, and check if the list has any elements
		public static Boolean IsUserLoggedIn() {
			return GetUsersLoggedIn().Count > 0;
		}
		
		public static String GetCurrentUser() {
			return WindowsIdentity.GetCurrent().Name;
		}

		
		//Return local users
		public static List<UserData> GetAllUserData() {
			var users = new List<UserData>();
			
			var query = new SelectQuery("Win32_UserAccount");
			var searcher = new ManagementObjectSearcher(query);
            
			foreach (ManagementObject envVar in searcher.Get()) {
				var userData = new UserData(envVar["Name"].ToString(), envVar["SID"].ToString());
				users.Add(userData);
			}
			
			return users;
		}
		
		//Return how long the logged in user is inactive for in seconds
		public static int GetUserInactivityTime() {
			uint idleTime = 0;
			var lastInputInfo = new LASTINPUTINFO();
			lastInputInfo.cbSize = (uint)Marshal.SizeOf( lastInputInfo );
			lastInputInfo.dwTime = 0;
	
			uint envTicks = (uint)Environment.TickCount;
	
	        if ( GetLastInputInfo( ref lastInputInfo ) ) {
				uint lastInputTick = lastInputInfo.dwTime;
	
				idleTime = envTicks - lastInputTick;
			}
			
			return (int)idleTime/1000;
		}	
		
		//Get a list of all users logged in
		public static List<String> GetUsersLoggedIn() {
			var users = new List<String>();
			var sessionIds = GetSessionIds();
			
			foreach(int sessionId in sessionIds) {
				if(!GetUserNameFromSessionId(sessionId, false).Equals("SYSTEM"))
					users.Add(GetUserNameFromSessionId(sessionId, false));
			}
			
			return users;
		}	
		
		//Get all session Ids from running processes
		public static List<int> GetSessionIds() {
			var sessionIds = new List<int>();
			var properties = new[] {"SessionId"};
			
			var query = new SelectQuery("Win32_Process", "", properties); //SessionId
			var searcher = new ManagementObjectSearcher(query);
            
			foreach (ManagementObject envVar in searcher.Get()) {
				try {
					if(!sessionIds.Contains(int.Parse(envVar["SessionId"].ToString()))) {
						sessionIds.Add(int.Parse(envVar["SessionId"].ToString()));
					}
				} catch (Exception ex) {
					LogHandler.Log(LOG_NAME, "Unable to parse Session Id");
					LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
				}
			}	
			return sessionIds;			
		}
		
		//Convert a session ID to a username
		//https://stackoverflow.com/questions/19487541/get-windows-user-name-from-sessionid
		public static String GetUserNameFromSessionId(int sessionId, bool prependDomain) {
			IntPtr buffer;
			int strLen;
			string username = "SYSTEM";
			if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1) {
				username = Marshal.PtrToStringAnsi(buffer);
				WTSFreeMemory(buffer);
				if (prependDomain) {
					if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) && strLen > 1) {
						username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
						WTSFreeMemory(buffer);
					}
				}
			}
			return username;
		}
		
		public static String GetUserProfilePath(String sid) {
			return RegistryHandler.GetRegisitryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + sid + @"\", "ProfileImagePath");
		}		
		
		//Completely purge a user from windows
		public static Boolean PurgeUser(UserData user, Boolean deleteData) {
			LogHandler.Log(LOG_NAME, "Purging " + user.GetName() + " from system");
			if(deleteData) {
				if(UnregisterUser(user.GetName())) {
					if(RemoveUserProfile(user.GetSID())) {
						return CleanUserRegistryEntries(user.GetSID());
					}
				}
				return false;
			} else {
				return UnregisterUser(user.GetName());
			}
		}
	
		
		//Unregister a user from windows
		public static Boolean UnregisterUser(String user) {
			try {
				var userDir = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
				var userToDelete = userDir.Children.Find(user);
				
				userDir.Children.Remove(userToDelete);
				return true;
				
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Unable to unregister user");
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}
			return false;			
		}
		
		//Delete user profile
		public static Boolean RemoveUserProfile(String sid) {
			
			try {
				String path = GetUserProfilePath(sid);
				LogHandler.Log(LOG_NAME, "User path: " + path);
				if(path != null) {
					TakeOwnership(path);
					resetRights(path);
					RemoveWriteProtection(path);
					Directory.Delete(path, true);
					return true;
				}
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Unable to remove user data");
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}
			return false;
		}
		
		//Clean all registry entries of a user
		public static Boolean CleanUserRegistryEntries(String sid) {
			return RegistryHandler.DeleteFolder(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + sid + @"\");
		}
		
		public static void TakeOwnership(String path) {

			using (new ProcessPrivileges.PrivilegeEnabler(Process.GetCurrentProcess(), ProcessPrivileges.Privilege.TakeOwnership)){
			    var directoryInfo = new DirectoryInfo(path);
			    DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
			    directorySecurity.SetOwner(WindowsIdentity.GetCurrent().User);
			    Directory.SetAccessControl(path, directorySecurity);    
			}

		}
		
		private static DirectorySecurity removeExplicitSecurity(DirectorySecurity directorySecurity) {
			var rules = directorySecurity.GetAccessRules(true, false, typeof(System.Security.Principal.NTAccount));
			foreach (FileSystemAccessRule rule in rules)
				directorySecurity.RemoveAccessRule(rule);
			return directorySecurity;
		}
		
		public static void resetRights(String path) {
			var directoryInfo = new DirectoryInfo(path);
			var directorySecurity = directoryInfo.GetAccessControl();
			directorySecurity = removeExplicitSecurity(directorySecurity);
			Directory.SetAccessControl(path, directorySecurity);
		}
		
		public static void RemoveWriteProtection(String path) {
			 var directoryInfo = new DirectoryInfo(path);
			 directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
		}		
		

	}
}