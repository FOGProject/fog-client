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
using System.Management;

namespace FOG
{
    /// <summary>
    /// Change the resolution of the display
    /// </summary>
    public class DisplayManager : AbstractModule
    {
        private Display display;
		
        public DisplayManager()
        {
            Name = "DisplayManager";
            Description = "Change the resolution of the display";	
            this.display = new Display();
        }
		
        protected override void doWork()
        {
            display.LoadDisplaySettings();
            if (display.PopulatedSettings) {
                //Get task info
                var taskResponse = CommunicationHandler.GetResponse("/service/displaymanager.php", true);
	
                if (!taskResponse.Error) {
	
                    try {
                        int x = int.Parse(taskResponse.getField("#x"));
                        int y = int.Parse(taskResponse.getField("#y"));
                        int r = int.Parse(taskResponse.getField("#r"));
                        if (getDisplays().Count > 0)
                            changeResolution(getDisplays()[0], x, y, r);
                        else
                            changeResolution("", x, y, r);
                    } catch (Exception ex) {
                        LogHandler.Log(Name, "ERROR");
                        LogHandler.Log(Name, ex.Message);
                    }
                }
            } else {
                LogHandler.Log(Name, "Settings are not populated; will not attempt to change resolution");
            }
        }
		
        //Change the resolution of the screen
        private void changeResolution(String device, int width, int height, int refresh)
        {
            if (!(width.Equals(display.Configuration.dmPelsWidth) && height.Equals(display.Configuration.dmPelsHeight) && refresh.Equals(display.Configuration.dmDisplayFrequency))) {
                LogHandler.Log(Name, "Current Resolution: " + display.Configuration.dmPelsWidth + " x " +
                display.Configuration.dmPelsHeight + " " + display.Configuration.dmDisplayFrequency + "hz");
                LogHandler.Log(Name, "Attempting to change resoltution to " + width + " x " + height + " " + refresh + "hz");
                LogHandler.Log(Name, "Display name: " + device);
				
                display.ChangeResolution(device, width, height, refresh);
				
            } else {
                LogHandler.Log(Name, "Current resolution is already set correctly");
            }
        }
		
        private List<String> getDisplays()
        {
            var displays = new List<String>();			
            var monitorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");
			
            foreach (var monitor in monitorSearcher.Get()) {
                displays.Add(monitor["Name"].ToString());
            }
            return displays;
        }
		
		
    }
}