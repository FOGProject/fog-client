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
using System.Linq;
using System.Management;
using FOG.Handlers;

namespace FOG.Modules.DisplayManager.Windows
{
    internal class WindowsDisplay : IDisplay
    {
        private readonly Display _display;
        private readonly string Name = "DisplayManager";

        public WindowsDisplay()
        {
            _display = new Display();
        }

        public void ChangeResolution(string device, int width, int height, int refresh)
        {
            _display.LoadDisplaySettings();
            if (!_display.PopulatedSettings)
            {
                Log.Error(Name, "Could not populate " + device + " settings");
                return;
            }

            if (!width.Equals(_display.Configuration.dmPelsWidth) &&
                !height.Equals(_display.Configuration.dmPelsHeight) &&
                !refresh.Equals(_display.Configuration.dmDisplayFrequency))
            {
                Log.Entry(Name, "Resolution is already configured correctly");
                return;
            }

            try
            {
                Log.Entry(Name,
                    string.Format("Current Resolution: {0} x {1} {2}hz", _display.Configuration.dmPelsWidth,
                        _display.Configuration.dmPelsHeight, _display.Configuration.dmDisplayFrequency));
                Log.Entry(Name,
                    string.Format("Attempting to change resoltution to {0} x {1} {2}hz", width, height, refresh));
                Log.Entry(Name, "Display name: " + device);

                _display.ChangeResolution(device, width, height, refresh);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        public List<string> GetDisplays()
        {
            var monitorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");

            return
                (from ManagementBaseObject monitor in monitorSearcher.Get() select monitor["Name"].ToString()).ToList();
        }
    }
}