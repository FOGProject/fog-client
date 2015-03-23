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
using Microsoft.Win32;

namespace FOG.Handlers
{
    /// <summary>
    /// Handle all interaction with the registry
    /// </summary>
    public static class RegistryHandler
    {

        private const String LOG_NAME = "RegistryHandler";
        private static String root = "";
		
        private static void updateRoot()
        {
            if (GetRegisitryValue(@"Software\Wow6432Node\FOG\", "Server") != null)
            {
                root = @"Software\Wow6432Node\FOG\";
                LogHandler.Log(LOG_NAME, "64 bit registry detected");				
            }
            else
            {
                root = @"Software\FOG\";
                LogHandler.Log(LOG_NAME, "32 bit registry detected");
            }
        }
		
        public static String GetRegisitryValue(String keyPath, String keyName)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key != null)
                {
                    String keyValue = key.GetValue(keyName).ToString();
                    key.Close();
                    if (keyValue != null)
                    {
                        return keyValue.Trim();
                    }
                }	
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Error retrieving " + keyPath + keyName);
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }
            return null;
        }
		
        public static String GetSystemSetting(String name)
        {
            return GetRegisitryValue(GetRoot(), name);
        }
		
        public static Boolean SetSystemSetting(String keyName, String value)
        {
            return SetRegistryValue(GetRoot(), keyName, value);
        }
		
        public static String GetModuleSetting(String module, String keyName)
        {
            return GetRegisitryValue(GetRoot() + @"\" + module, keyName);
        }
		
        public static Boolean SetModuleSetting(String module, String keyName, String value)
        {
            return SetRegistryValue(GetRoot() + @"\" + module, keyName, value);
        }
		
        public static Boolean DeleteModuleSetting(String module, String keyName)
        {
            return DeleteKey(GetRoot() + @"\" + module, keyName);
        }
		
        public static Boolean DeleteModule(String module)
        {
            return DeleteFolder(GetRoot() + @"\" + module);
        }
		
		
        public static Boolean SetRegistryValue(String keyPath, String keyName, String value)
        {
			
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(keyPath, true);
                key.CreateSubKey(keyName);
                key.SetValue(keyName, value);
                key.Close();
				
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Error setting " + keyPath + keyName);
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }			
			
            return false;
        }
		
		
        public static Boolean DeleteFolder(String path)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(path, true);
                if (key != null)
                {
                    key.DeleteSubKeyTree(path);
                    key.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Error while trying to remove " + path);
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }
			
            return false;
        }
		
        public static Boolean DeleteKey(String keyPath, String keyName)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(keyPath, true);
                if (key != null)
                {
                    key.DeleteValue(keyName);
                    key.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Error while trying to remove " + keyPath);
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }
			
            return false;			
        }

        public static String GetRoot()
        { 
            if (root.Equals(""))
            {
                updateRoot();
            }
			
            return root;
        }
		
    }
}