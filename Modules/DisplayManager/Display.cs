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

namespace FOG.Modules
{
    /// <summary>
    ///     Contains functionality to resize display
    /// </summary>
    public class Display
    {
        private const string LOG_NAME = "DisplayManager:Display";
        // Cannot use auto-populated getters and setters as it would turn this property into a method
        public User_32.DEVMODE1 Configuration;

        public Display()
        {
            Configuration = new User_32.DEVMODE1
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
            if (User_32.EnumDisplaySettings(null, User_32.ENUM_CURRENT_SETTINGS, ref Configuration) != 0)
                return true;

            LogHandler.Log(LOG_NAME, "Unable to load display settings");
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
            LogHandler.Log(LOG_NAME, "Testing resolution to ensure it is compatible");
            var changeStatus = User_32.ChangeDisplaySettings(ref Configuration, User_32.CDS_TEST);

            if (changeStatus.Equals(User_32.DISP_CHANGE_FAILED))
                LogHandler.Log(LOG_NAME, "Failed");
            else
            {
                LogHandler.Log(LOG_NAME, "Changing resolution");
                changeStatus = User_32.ChangeDisplaySettings(ref Configuration, User_32.CDS_UPDATEREGISTRY);

                if (changeStatus.Equals(User_32.DISP_CHANGE_SUCCESSFUL))
                    LogHandler.Log(LOG_NAME, "Success");
                else if (changeStatus.Equals(User_32.DISP_CHANGE_RESTART))
                    LogHandler.Log(LOG_NAME, "Success, requires reboot");
                else if (changeStatus.Equals(User_32.DISP_CHANGE_FAILED))
                    LogHandler.Log(LOG_NAME, "Failed");
            }
        }
    }
}