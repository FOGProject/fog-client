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

using System.Runtime.InteropServices;
using FOG.Handlers;

namespace FOG.Modules.DisplayManager
{
    /// <summary>
    ///     Contains functionality to resize display
    /// </summary>
    public class Display
    {
        private const string LogName = "DisplayManager:Display";
        // Cannot use auto-populated getters and setters as it would turn this property into a method
        public User32.Devmode1 Configuration;

        public Display()
        {
            Configuration = new User32.Devmode1
            {
                dmDeviceName = new string(new char[32]),
                dmFormName = new string(new char[32])
            };
            Configuration.dmSize = (short) Marshal.SizeOf(Configuration);
            PopulatedSettings = LoadDisplaySettings();
        }

        public bool PopulatedSettings { get; private set; }

        /// <summary>
        ///     Load the current display settings
        /// </summary>
        /// <returns>True if the settings were successfully loaded</returns>
        public bool LoadDisplaySettings()
        {
            if (User32.EnumDisplaySettings(null, User32.EnumCurrentSettings, ref Configuration) != 0)
                return true;

            Log.Error(LogName, "Unable to load display settings");
            return false;
        }

        /// <summary>
        ///     Resize a given display
        /// </summary>
        /// <param name="device">The display to resize</param>
        /// <param name="width">The new width in pixels</param>
        /// <param name="height">The new height in pixels</param>
        /// <param name="refresh">The new refresh rate</param>
        public void ChangeResolution(string device, int width, int height, int refresh)
        {
            if (!PopulatedSettings) return;

            Configuration.dmPelsWidth = width;
            Configuration.dmPelsHeight = height;
            Configuration.dmDisplayFrequency = refresh;
            Configuration.dmDeviceName = device;

            //Test changing the resolution first
            Log.Entry(LogName, "Testing resolution to ensure it is compatible");
            var changeStatus = User32.ChangeDisplaySettings(ref Configuration, User32.CdsTest);

            if (changeStatus.Equals(User32.DispChangeFailed))
                Log.Entry(LogName, "Failed");
            else
            {
                Log.Entry(LogName, "Changing resolution");
                changeStatus = User32.ChangeDisplaySettings(ref Configuration, User32.CdsUpdateregistry);

                if (changeStatus.Equals(User32.DispChangeSuccessful))
                    Log.Entry(LogName, "Success");
                else if (changeStatus.Equals(User32.DispChangeRestart))
                    Log.Entry(LogName, "Success, requires reboot");
                else if (changeStatus.Equals(User32.DispChangeFailed))
                    Log.Entry(LogName, "Failed");
            }
        }
    }
}