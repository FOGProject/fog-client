
namespace FOG.Modules.PrinterManager
{
    public class CUPSPrinter : Printer
    {
        public CUPSPrinter(string name, string file, string ip, bool defaulted)
        {
            Name = name;
            File = file;
            Default = defaulted;
            IP = ip;
            LogName = "CUPSPrinter";
        }

        public override void Add(PrintManagerBridge instance)
        {
            instance.Add(this);
        }
    }
}
