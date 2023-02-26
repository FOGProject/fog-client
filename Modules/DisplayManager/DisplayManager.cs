/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
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
using Zazzles;
using Zazzles.Middleware;
using Zazzles.Modules;


namespace FOG.Modules.DisplayManager
{
    /// <summary>
    ///     Change the resolution of the display
    /// </summary>
    public class DisplayManager : AbstractModule<DataContracts.DisplayManager>
    {
        private Display _display;

        public DisplayManager()
        {
            Name = "DisplayManager";
            Compatiblity = Settings.OSType.Windows;
        }

        protected override void DoWork(Response data, DataContracts.DisplayManager msg)
        {
            if(_display == null)
                _display = new Display();

            _display.LoadDisplaySettings();
            if (_display.PopulatedSettings)
            {
                if (msg.X <= 0 || msg.Y <= 0)
                {
                    Log.Error(Name, "Invalid settings provided");
                    return;
                }
                ChangeResolution(GetDisplays().Count > 0 ? GetDisplays()[0] : "", msg.X, msg.Y, msg.R);
            }
            else
                Log.Error(Name, "Settings are not populated; will not attempt to change resolution");
        }

        //Change the resolution of the screen
        private void ChangeResolution(string device, int width, int height, int refresh)
        {

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
                    $"Current Resolution: {_display.Configuration.dmPelsWidth} x {_display.Configuration.dmPelsHeight} {_display.Configuration.dmDisplayFrequency}hz");
                Log.Entry(Name, $"Attempting to change resoltution to {width} x {height} {refresh}hz");
                Log.Entry(Name, "Display name: " + device);

                _display.ChangeResolution(device, width, height, refresh);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);

            }
        }

        private static List<string> GetDisplays()
        {
            var monitorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");

            return (from ManagementBaseObject monitor in monitorSearcher.Get() select monitor["Name"].ToString()).ToList();
        }
    }
}