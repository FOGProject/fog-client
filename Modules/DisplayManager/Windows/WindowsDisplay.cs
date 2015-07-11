using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using FOG.Handlers;

namespace FOG.Modules.DisplayManager.Windows
{
    class WindowsDisplay : IDisplay
    {
        private string Name = "DisplayManager";
        private readonly Display _display;

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
                Log.Entry(Name, string.Format("Current Resolution: {0} x {1} {2}hz", _display.Configuration.dmPelsWidth, _display.Configuration.dmPelsHeight, _display.Configuration.dmDisplayFrequency));
                Log.Entry(Name, string.Format("Attempting to change resoltution to {0} x {1} {2}hz", width, height, refresh));
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

            return (from ManagementBaseObject monitor in monitorSearcher.Get() select monitor["Name"].ToString()).ToList();
        }
    }
}
