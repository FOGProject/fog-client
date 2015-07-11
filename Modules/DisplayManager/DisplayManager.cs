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
using FOG.Handlers;
using FOG.Handlers.Middleware;
using FOG.Modules.DisplayManager.Linux;
using FOG.Modules.DisplayManager.Mac;
using FOG.Modules.DisplayManager.Windows;


namespace FOG.Modules.DisplayManager
{
    /// <summary>
    ///     Change the resolution of the display
    /// </summary>
    public class DisplayManager : AbstractModule
    {
        private IDisplay _instance;

        public DisplayManager()
        {
            Name = "DisplayManager";
            Compatiblity = Settings.OSType.Windows;

            switch (Settings.OS)
            {
                case Settings.OSType.Mac:
                    _instance = new MacDisplay();
                    break;
                case Settings.OSType.Linux:
                    _instance = new LinuxDisplay();
                    break;
                default:
                    _instance = new WindowsDisplay();
                    break;
            }
        }

        protected override void DoWork()
        {
            //Get task info
            var response = Communication.GetResponse("/service/displaymanager.php", true);

            if (response.Error) return;

            try
            {
                var x = int.Parse(response.GetField("#x"));
                var y = int.Parse(response.GetField("#y"));
                var r = int.Parse(response.GetField("#r"));

                ChangeResolution(GetDisplays().Count > 0 ? GetDisplays()[0] : "", x, y, r);
            }
            catch (Exception ex)
            {
                Log.Error(Name, ex);
            }
        }

        //Change the resolution of the screen
        private void ChangeResolution(string device, int width, int height, int refresh)
        {
            try
            {
                _instance.ChangeResolution(device, width, height, refresh);

            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not change resolution");
                Log.Error(Name, ex);
            }
        }

        private List<string> GetDisplays()
        {
            try
            {
                return _instance.GetDisplays();
            }
            catch (Exception ex)
            {
                Log.Error(Name, "Could not get displays");
                Log.Error(Name, ex);
            }

            return new List<string>();
        }
    }
}