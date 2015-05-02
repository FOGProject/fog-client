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
    ///     Handle all interaction with the registry
    /// </summary>
    public static class RegistryHandler
    {
        private const string LOG_NAME = "RegistryHandler";
        private static string root = "";

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

        public static string GetRegisitryValue(string keyPath, string keyName)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key != null)
                {
                    var keyValue = key.GetValue(keyName).ToString();
                    key.Close();
                    return keyValue.Trim();
                }
            }
            catch (Exception ex)
            {
                LogHandler.Log(LOG_NAME, "Error retrieving " + keyPath + keyName);
                LogHandler.Log(LOG_NAME, "ERROR: " + ex.Message);
            }
            return null;
        }

        public static string GetSystemSetting(string name)
        {
            return GetRegisitryValue(GetRoot(), name);
        }

        public static bool SetSystemSetting(string keyName, string value)
        {
            return SetRegistryValue(GetRoot(), keyName, value);
        }

        public static string GetModuleSetting(string module, string keyName)
        {
            return GetRegisitryValue(GetRoot() + @"\" + module, keyName);
        }

        public static bool SetModuleSetting(string module, string keyName, string value)
        {
            return SetRegistryValue(GetRoot() + @"\" + module, keyName, value);
        }

        public static bool DeleteModuleSetting(string module, string keyName)
        {
            return DeleteKey(GetRoot() + @"\" + module, keyName);
        }

        public static bool DeleteModule(string module)
        {
            return DeleteFolder(GetRoot() + @"\" + module);
        }

        public static bool SetRegistryValue(string keyPath, string keyName, string value)
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

        public static bool DeleteFolder(string path)
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

        public static bool DeleteKey(string keyPath, string keyName)
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

        public static string GetRoot()
        {
            if (root.Equals(""))
            {
                updateRoot();
            }

            return root;
        }
    }
}