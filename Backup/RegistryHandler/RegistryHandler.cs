
using System;
using Microsoft.Win32;
using System.Collections.Generic;

namespace FOG {
	/// <summary>
	/// Handle all interaction with the registry
	/// </summary>
	public static class RegistryHandler {

		private const String LOG_NAME = "RegistryHandler";
		private static String root = "";
		
		private static void updateRoot() {
			if(GetRegisitryValue(@"Software\Wow6432Node\FOG\", "Server") != null) {
				root = @"Software\Wow6432Node\FOG\";
				LogHandler.Log(LOG_NAME, "64 bit registry detected");				
			} else {
				root = @"Software\FOG\";
				LogHandler.Log(LOG_NAME, "32 bit registry detected");
			}
		}
		
		public static String GetRegisitryValue(String keyPath, String keyName) {
			try {
				RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
	            if (key != null) {
	            	String keyValue = key.GetValue(keyName).ToString();
	            	if (keyValue != null) {
	            		return keyValue.Trim();
	                }
	            }	
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error retrieving " + keyPath + keyName);
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}
			return null;
		}
		
		public static String GetSystemSetting(String name) {
			return GetRegisitryValue(GetRoot(), name);
		}
		
		public static Boolean SetSystemSetting(String keyName, String value) {
			return SetRegistryValue(GetRoot(), keyName, value);
		}
		
		public static String GetModuleSetting(String module, String keyName) {
			return GetRegisitryValue(GetRoot() + @"\" + module, keyName);
		}
		
		public static Boolean SetModuleSetting(String module, String keyName, String value) {
			return SetRegistryValue(GetRoot() + @"\" + module, keyName, value);
		}
		
		public static Boolean DeleteModuleSetting(String module, String keyName) {
			return DeleteKey(GetRoot() + @"\" + module, keyName);
		}
		
		public static Boolean DeleteModule(String module) {
			return DeleteFolder(GetRoot() + @"\" + module);
		}
		
		
		public static Boolean SetRegistryValue(String keyPath, String keyName, String value) {
			
			try {
				RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
				
				key.CreateSubKey(keyName);
				key.SetValue(keyName, value);
				
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error setting " + keyPath + keyName);
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}			
			
			return false;
		}
		
		
		public static Boolean DeleteFolder(String path) {
			try {
				RegistryKey key = Registry.LocalMachine.OpenSubKey(path, true);
				if (key != null) {
					key.DeleteSubKeyTree(path);
					return true;
				}
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error while trying to remove " + path);
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}
			
			return false;
		}
		
		public static Boolean DeleteKey(String keyPath, String keyName) {
			try {
				RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true);
				if (key != null) {
					key.DeleteValue(keyName);
					return true;
				}
			} catch (Exception ex) {
				LogHandler.Log(LOG_NAME, "Error while trying to remove " + keyPath);
				LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
			}
			
			return false;			
		}

		public static String GetRoot() { 
			if(root.Equals("")) {
				updateRoot();
			}
			
			return root;
		}
		
	}
}