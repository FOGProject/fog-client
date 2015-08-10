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
        private const string LogName = "RegistryHandler";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyPath">The path to the registry key</param>
        /// <param name="keyName">The name of the registry key</param>
        /// <returns>The value of the key</returns>
        public static string GetRegisitryValue(string keyPath, string keyName)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(keyPath);

                if (key == null) throw new NullReferenceException("Null key");
                
                var keyValue = key.GetValue(keyName).ToString();
                key.Close();
                return keyValue.Trim();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, string.Format("Could not retrieve {0}{1}", keyPath, keyName));
                Log.Error(LogName, ex);
            }
            return null;
        }

        public static bool SetRegistryValue(string keyPath, string keyName, string value)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(keyPath, true);
                if (key == null) throw new NullReferenceException("Null key");

                key.CreateSubKey(keyName);
                key.SetValue(keyName, value);
                key.Close();
            }
            catch (Exception ex)
            {
                Log.Error(LogName, string.Format("Could not set {0}{1}", keyPath, keyName));
                Log.Error(LogName, ex);
            }

            return false;
        }

        public static bool DeleteFolder(string path)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(path, true);
                if (key == null) throw new NullReferenceException("Null key");

                key.DeleteSubKeyTree(path);
                key.Close();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, string.Format("Could not delete {0}", path));
                Log.Error(LogName, ex);
            }

            return false;
        }

        public static bool DeleteKey(string keyPath, string keyName)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(keyPath, true);
                if (key == null) throw new NullReferenceException("Null key");

                key.DeleteValue(keyName);
                key.Close();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, string.Format("Could not delete {0}", keyPath));
                Log.Error(LogName, ex);
            }

            return false;
        }
    }
}