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
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace FOG.Handlers
{
    public static class Settings
    {
        public enum OSType
        {
            All,
            Windows,
            Nix,
            Mac,
            Linux
        }

        private const string LogName = "Settings";

        private static readonly string _file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
        private static JObject _data;
        public static OSType OS { get; private set; }

        static Settings()
        {
            try
            {
                _data = JObject.Parse(File.ReadAllText(_file));
                Log.Entry(LogName, _data.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to load settings");
                Log.Error(LogName, ex);
                _data = new JObject();
            }

            var pid = Environment.OSVersion.Platform;

            switch (pid)
            {
                case PlatformID.MacOSX:
                    OS = OSType.Mac;
                    break;
                case PlatformID.Unix:
                    OS = OSType.Linux;
                    break;
                default:
                    OS = OSType.Windows;
                    break;
            }
        }

        public static bool IsCompatible(OSType type)
        {
            if (type == OSType.All)
                return true;

            if (type == OS)
                return true;

            if (type == OSType.Linux || type == OSType.Mac && OS == OSType.Nix)
                return true;

            return false;
        }

        private static bool Save()
        {
            try
            {
                File.WriteAllText(_file, _data.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to save settings");
                Log.Error(LogName, ex);
            }

            return false;
        }

        public static string Get(string key)
        {
            var value = _data.GetValue(key);
            Log.Entry(LogName, "Retrieived " + key + " = " + value);
            return (value == null) ? "" : value.ToString();
        }

        public static void Set(string key, JToken value)
        {
            _data.Add(key, value);
            Save();
        }
    }
}
