using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace FOG.Modules.PrinterManager
{
    class WindowsPrinterManager : IPrinterManager
    {
        public List<string> GetPrinters()
        {
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            return (from ManagementBaseObject printer in printerQuery.Get() select printer.GetPropertyValue("name").ToString()).ToList();
        }
    }
}
