using FOG.Handlers;

namespace FOG.Modules.PrinterManager
{
    // ReSharper disable once InconsistentNaming
    public class iPrintPrinter : Printer
    {
        public iPrintPrinter(string name, string ip, string port, bool defaulted)
        {
            Name = name;
            IP = ip;
            Port = port;
            Default = defaulted;
            LogName = "iPrinter";
        }

        public override void Add(PrintManagerBridge instance)
        {
            instance.Add(this);
        }
    }
}
