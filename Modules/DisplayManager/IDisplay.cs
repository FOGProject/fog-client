using System.Collections.Generic;

namespace FOG.Modules.DisplayManager
{
    interface IDisplay
    {
        void ChangeResolution(string device, int width, int height, int refresh);
        List<string> GetDisplays();
    }
}
