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