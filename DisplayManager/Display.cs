using System;
using System.Runtime.InteropServices;

namespace FOG
{
    /// <summary>
    /// Contains functionality to resize display
    /// </summary>
    public class Display
    {

        private const String LOG_NAME = "DisplayManager:Display";
        public Boolean PopulatedSettings { get; private set; }
        
        // Cannot use auto-populated getters and setters as it would turn this property into a method
        public User_32.DEVMODE1 Configuration;
		
        public Display()
        {
            Configuration = new User_32.DEVMODE1();
            Configuration.dmDeviceName = new String(new char[32]);
            Configuration.dmFormName = new String(new char[32]);
            Configuration.dmSize = (short)Marshal.SizeOf(Configuration);
            PopulatedSettings = LoadDisplaySettings();
        }
		
        /// <summary>
        /// Load the current display settings
        /// </summary>
        /// <returns>True if the settings were successfully loaded</returns>
        public Boolean LoadDisplaySettings()
        {
            if (User_32.EnumDisplaySettings(null, User_32.ENUM_CURRENT_SETTINGS, ref Configuration) != 0)
            {
                return true;
            }			
            LogHandler.Log(LOG_NAME, "Unable to load display settings");
            return false;
        }
		
        /// <summary>
        /// Resize a given display
        /// </summary>
        /// <param name="device">The display to resize</param>
        /// <param name="width">The new width in pixels</param>
        /// <param name="height">The new height in pixels</param>
        /// <param name="refresh">The new refresh rate</param>
        public void ChangeResolution(String device, int width, int height, int refresh)
        {
            if (PopulatedSettings)
            {
                Configuration.dmPelsWidth = width;
                Configuration.dmPelsHeight = height;
                Configuration.dmDisplayFrequency = refresh;
                Configuration.dmDeviceName = device;
				
                //Test changing the resolution first
                LogHandler.Log(LOG_NAME, "Testing resolution to ensure it is compatible");
                int changeStatus = User_32.ChangeDisplaySettings(ref Configuration, User_32.CDS_TEST);
				
                if (changeStatus.Equals(User_32.DISP_CHANGE_FAILED))
                {
                    LogHandler.Log(LOG_NAME, "Failed");
                }
                else
                {
                    LogHandler.Log(LOG_NAME, "Changing resolution");
                    changeStatus = User_32.ChangeDisplaySettings(ref Configuration, User_32.CDS_UPDATEREGISTRY);
					
                    if (changeStatus.Equals(User_32.DISP_CHANGE_SUCCESSFUL))
                    {
                        LogHandler.Log(LOG_NAME, "Success");
                    }
                    else if (changeStatus.Equals(User_32.DISP_CHANGE_RESTART))
                    {
                        LogHandler.Log(LOG_NAME, "Success, requires reboot");
                    }
                    else if (changeStatus.Equals(User_32.DISP_CHANGE_FAILED))
                    {
                        LogHandler.Log(LOG_NAME, "Failed");
                    }
                }
				
            }
        }
    }
}
