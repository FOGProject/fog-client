using System;
using System.Collections.Generic;

namespace FOG.Modules.DisplayManager.Linux
{
    class LinuxDisplay : IDisplay
    {
        private string Name = "DisplayManager";


        public void ChangeResolution(string device, int width, int height, int refresh)
        {
            throw new NotImplementedException();
        }

        public List<string> GetDisplays()
        {
            throw new NotImplementedException();
        }
    }
}
