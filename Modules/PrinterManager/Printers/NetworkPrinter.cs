using FOG.Handlers;

namespace FOG.Modules.PrinterManager
{
    public class NetworkPrinter : Printer
    {
        public NetworkPrinter(string name, string ip, string port, bool defaulted)
        {
            Name = name;
            IP = ip;
            Port = port;
            Default = defaulted;
            LogName = "NetworkPrinter";
        }

        public override void Add(PrintManagerBridge instance)
        {
            if (string.IsNullOrEmpty(IP) || !Name.StartsWith("\\\\")) return;

            instance.Add(this);
        }
    }
}
