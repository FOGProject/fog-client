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


namespace FOG.Modules.DisplayManager
{
    /// <summary>
    ///     Change the resolution of the display
    /// </summary>
    public class DisplayManager : AbstractModule
    {
        private readonly Display _display;

        public DisplayManager()
        {
            Name = "DisplayManager";
            Description = "Change the resolution of the display";
            _display = new Display();
        }

        protected override void DoWork()
        {
            _display.LoadDisplaySettings();
            if (_display.PopulatedSettings)
            {
                //Get task info
                var taskResponse = CommunicationHandler.GetResponse("/service/displaymanager.php", true);

                if (taskResponse.Error) return;

                try
                {
                    var x = int.Parse(taskResponse.GetField("#x"));
                    var y = int.Parse(taskResponse.GetField("#y"));
                    var r = int.Parse(taskResponse.GetField("#r"));

                    ChangeResolution(GetDisplays().Count > 0 ? GetDisplays()[0] : "", x, y, r);
                }
                catch (Exception ex)
                {
                    LogHandler.Log(Name, "ERROR");
                    LogHandler.Log(Name, ex.Message);
                }
            }
            else
            {
                LogHandler.Error(Name, "Settings are not populated; will not attempt to change resolution");
            }
        }

        //Change the resolution of the screen
        private void ChangeResolution(string device, int width, int height, int refresh)
        {

            if (!width.Equals(_display.Configuration.dmPelsWidth) && 
                !height.Equals(_display.Configuration.dmPelsHeight) &&
                !refresh.Equals(_display.Configuration.dmDisplayFrequency))
            {
                LogHandler.Log(Name, "Resolution is already configured correctly");
                return;
            }

            try
            {
                LogHandler.Log(Name, string.Format("Current Resolution: {0} x {1} {2}hz", _display.Configuration.dmPelsWidth, _display.Configuration.dmPelsHeight, _display.Configuration.dmDisplayFrequency));
                LogHandler.Log(Name, string.Format("Attempting to change resoltution to {0} x {1} {2}hz", width, height, refresh));
                LogHandler.Log(Name, "Display name: " + device);

                _display.ChangeResolution(device, width, height, refresh);
            }
            catch (Exception ex)
            {
                LogHandler.Error(Name, ex.Message);

            }
        }

        private static List<string> GetDisplays()
        {
            var monitorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");

            return (from ManagementBaseObject monitor in monitorSearcher.Get() select monitor["Name"].ToString()).ToList();
        }
    }
}