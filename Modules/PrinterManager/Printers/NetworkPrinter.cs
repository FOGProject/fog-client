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
            Log.Entry(LogName, "Attempting to add printer:");
            Log.Entry(LogName, string.Format("--> Name = {0}", Name));
            Log.Entry(LogName, string.Format("--> IP = {0}", IP));
            Log.Entry(LogName, string.Format("--> Port = {0}", Port));

            if (string.IsNullOrEmpty(IP) || !Name.StartsWith("\\\\")) return;

            instance.Add(this);
        }
    }
}
